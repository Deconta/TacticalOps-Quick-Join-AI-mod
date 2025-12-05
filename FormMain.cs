#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading; 
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection; 
using System.Net.Http;
using System.Text.Json;
using System.Runtime.InteropServices;
using TacticalOpsQuickJoin.Properties;

namespace TacticalOpsQuickJoin
{
    public partial class FormMain : Form
    {
        private static readonly HttpClient _httpClient = new();
        
        public List<MasterServer.MasterServerInfo> masterServers = [];
        private List<ServerData> servers = [];
        private readonly Dictionary<int, ServerData> _serverLookup = new();
        private readonly SemaphoreSlim _pingSemaphore = new(Constants.MAX_CONCURRENT_PINGS);
        private List<MapData> _mapList = [];
        private readonly Dictionary<string, MapData> _mapLookup = new();
        private Form? _mapPreviewWindow;
        private CancellationTokenSource? _hoverTokenSource;
        private readonly Dictionary<string, Image> _imageCache = new();
        private readonly HashSet<string> _favoriteServers = [];
        private System.Windows.Forms.Timer? _autoRefreshTimer;
        private ThemeManager? _themeManager;
        private bool _isRefreshing;
        private Font? _starFont;
        private bool _themeApplied;
        private System.Windows.Forms.Timer? _sortTimer;
        private bool _needsSort;
        private bool _isScrolling;
        private System.Windows.Forms.Timer? _scrollTimer;

        public FormMain()
        {

            InitializeComponent();

            this.MinimizeBox = true;
            this.MaximizeBox = false;

            EnableDoubleBuffering(serverListView);
            EnableDoubleBuffering(playerListView);
            EnableDoubleBuffering(serverSettingsView);
            
            serverListView.RowHeadersVisible = false;
            serverListView.AllowUserToResizeRows = false;
            
            _sortTimer = new System.Windows.Forms.Timer();
            _sortTimer.Interval = 2000;
            _sortTimer.Tick += (s, e) => 
            {
                _sortTimer?.Stop();
                if (_needsSort && !_isRefreshing)
                {
                    SortServerList();
                    _needsSort = false;
                }
            };
            
            _scrollTimer = new System.Windows.Forms.Timer();
            _scrollTimer.Interval = 300;
            _scrollTimer.Tick += (s, e) => 
            {
                _scrollTimer?.Stop();
                _isScrolling = false;
            };
            
            serverListView.Scroll += (s, e) => 
            {
                _isScrolling = true;
                CloseMapPreview();
                _scrollTimer?.Stop();
                _scrollTimer?.Start();
            };

            _starFont = UIConstants.Fonts.StarFont;
            
            // Initialize theme manager
            _themeManager = new ThemeManager(this, Settings.Default.darkMode);
            
            if (Controls.Find("menuStrip1", true).FirstOrDefault() is MenuStrip menuStrip)
            {
                _themeManager.ApplyToMenuStrip(menuStrip);
            }

            // Add ping column if needed
            if (serverListView.Columns.Count == 4)
            {
                serverListView.Columns.Add("PingColumn", "Ping");
            }
            
            // Add favorites column as first column
            if (serverListView.Columns.Count == 5)
            {
                var favColumn = new DataGridViewTextBoxColumn
                {
                    Name = "FavColumn",
                    HeaderText = "★",
                    Width = UIConstants.FAVORITES_COLUMN_WIDTH,
                    MinimumWidth = UIConstants.FAVORITES_COLUMN_WIDTH
                };
                serverListView.Columns.Insert(0, favColumn);
            }
            
            LoadFavorites();
            ApplyThemeToControls();
            InitializeAutoRefresh();
            
            this.KeyPreview = true;
            this.KeyDown += FormMain_KeyDown!;
            
            serverListView.CellDoubleClick += serverListView_CellDoubleClick!;
            serverListView.CellClick += serverListView_CellClick!;
            playerListView.CellMouseDown += playerListView_CellMouseDown!;
            serverListView.CellMouseEnter += serverListView_CellMouseEnter!;
            serverListView.CellMouseLeave += serverListView_CellMouseLeave!;
            
            _ = LoadMapDataAsync();

            ConfigureColumnWidths();

            // Configure column data types for sorting
            playerListScoreColumn.ValueType = typeof(Int32);
            playerListKillsColumn.ValueType = typeof(Int32);
            playerListDeathColumn.ValueType = typeof(Int32);
            playerListPingColumn.ValueType = typeof(Int32);
            playerListTeamColumn.ValueType = typeof(Int32);

            closeOnJoinToolStripMenuItem.Checked = Settings.Default.closeOnJoin;
            LoadMasterServersFromSettings();
            ConfigureStatusLabels();
        }

        private void ConfigureColumnWidths()
        {
            if (serverListView.Columns.Count >= 6)
            {
                serverListView.Columns[UIConstants.FAVORITES_COLUMN_INDEX].Width = UIConstants.FAVORITES_COLUMN_WIDTH;
                serverListView.Columns[1].Width = 240;
                serverListView.Columns[UIConstants.MAP_COLUMN_INDEX].Width = 130;
                serverListView.Columns[UIConstants.PLAYERS_COLUMN_INDEX].Width = 100;
                serverListView.Columns[UIConstants.PING_COLUMN_INDEX].Width = 50;
                serverListView.Columns[5].Width = 60;
            }
        }

        private void ConfigureStatusLabels()
        {
            lblNoResponse.Hide();
            lblNoPlayers.Hide();
            lblWaitingForResponse.Hide();
            
            if (_themeManager != null)
            {
                lblNoResponse.ForeColor = _themeManager.GetLabelColor("NoResponse");
                lblNoPlayers.ForeColor = _themeManager.GetLabelColor("NoPlayers");
                lblWaitingForResponse.ForeColor = _themeManager.GetLabelColor("WaitingForResponse");
                lblDownloadState.ForeColor = _themeManager.GetLabelColor("DownloadState");
            }
        }

        private void ApplyThemeToControls()
        {
            if (_themeManager == null || _themeApplied) return;
            
            _themeManager.ApplyTheme();
            _themeManager.ApplyToDataGridView(serverListView);
            _themeManager.ApplyToDataGridView(playerListView);
            _themeManager.ApplyToDataGridView(serverSettingsView);
            _themeManager.ApplyToButton(btnJoinServer);
            
            if (Controls.Find("menuStrip1", true).FirstOrDefault() is MenuStrip menuStrip)
            {
                _themeManager.ApplyToMenuStrip(menuStrip);
            }
            
            _themeApplied = true;
        }

        private async Task LoadMapDataAsync()
        {
            string localJsonPath = Path.Combine(Application.StartupPath, "maps.json");
            // Try LOCAL file FIRST (most up-to-date)
            if (File.Exists(localJsonPath))
            {
                try
                {
                    string localJsonString = File.ReadAllText(localJsonPath);
                    _mapList = JsonSerializer.Deserialize<List<MapData>>(localJsonString, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                    
                    BuildMapLookup();
                    return;
                }
                catch
                {
                    // Fall back to online
                }
            }

            // Try online source as fallback
            try
            {
                using (var cts = new CancellationTokenSource(Constants.MAP_DOWNLOAD_TIMEOUT))
                {
                    string jsonString = await _httpClient.GetStringAsync(Constants.MAP_JSON_URL, cts.Token);
                    
                    _mapList = JsonSerializer.Deserialize<List<MapData>>(jsonString, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                    
                    BuildMapLookup();
                    return;
                }
            }
            catch
            {
                // Continue to test data fallback
            }

            // Create test data as final fallback
            _mapList = new List<MapData>();
            var testMaps = new[]
            {
                "TO-Deck16][", "AoT Italy", "CoLD {X@C}", "TO-IcyFort", "Deadly Drought", 
                "WinterRansom", "TO-Blaze-Of-Glory", "CIA", "TO-Conundrum"
            };
            
            foreach (var name in testMaps)
            {
                _mapList.Add(new MapData { Name = name, PreviewBig = "LOCAL_IMAGE" });
            }
            BuildMapLookup();
        }
        
        private void BuildMapLookup()
        {
            _mapLookup.Clear();
            foreach (var map in _mapList)
            {
                var normalized = NormalizeMapName(map.Name);
                if (!_mapLookup.ContainsKey(normalized))
                    _mapLookup[normalized] = map;
                
                var baseName = map.Name.Split('-').Last().ToLowerInvariant();
                if (!_mapLookup.ContainsKey(baseName) && baseName.Length > 3)
                    _mapLookup[baseName] = map;
            }
        }
        
        // Handle map preview on hover
        private void serverListView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_isRefreshing || _isScrolling) return;
            if (e.RowIndex < 0 || e.ColumnIndex != UIConstants.MAP_COLUMN_INDEX) return;
            
            string? mapName = serverListView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
            if (string.IsNullOrEmpty(mapName)) return;
            
            if (_mapList.Count == 0) return;
            
            // Get server info for preview
            if (serverListView.Rows[e.RowIndex].Tag is not int serverId) return;
            if (!_serverLookup.TryGetValue(serverId, out var server)) return;
            if (server == null) return;

            var mapData = FindBestMapMatch(mapName);
            
            // Get best available preview URL (prefer small for speed)
            string? previewUrl = null;
            if (mapData != null)
            {
                previewUrl = !string.IsNullOrEmpty(mapData.PreviewSmall) ? mapData.PreviewSmall : mapData.Preview;
            }
            
            // Show "No preview" for Untitled or missing maps
            if (string.IsNullOrEmpty(previewUrl) || mapName.Contains("Untitled", StringComparison.OrdinalIgnoreCase))
            {
                _hoverTokenSource?.Cancel();
                _hoverTokenSource?.Dispose();
                _hoverTokenSource = new CancellationTokenSource();
                
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(Constants.MAP_PREVIEW_DELAY, _hoverTokenSource.Token);
                        if (!_hoverTokenSource.Token.IsCancellationRequested && !_isRefreshing && !_isScrolling)
                        {
                            this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                            {
                                if (!_isRefreshing && !_isScrolling)
                                    ShowMapPreview("NO_PREVIEW", mapName, server.ServerName, mapName);
                            });
                        }
                    }
                    catch (TaskCanceledException) { }
                }, _hoverTokenSource.Token);
                return;
            }

            _hoverTokenSource?.Cancel();
            _hoverTokenSource?.Dispose();
            _hoverTokenSource = new CancellationTokenSource();
            
            var finalPreviewUrl = previewUrl;
            var finalMapName = mapData?.Name ?? mapName;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Constants.MAP_PREVIEW_DELAY, _hoverTokenSource.Token);
                    
                    if (!_hoverTokenSource.Token.IsCancellationRequested && !_isRefreshing && !_isScrolling)
                    {
                        this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            if (!_isRefreshing && !_isScrolling)
                                ShowMapPreview(finalPreviewUrl, finalMapName, server.ServerName, mapName);
                        });
                    }
                }
                catch (TaskCanceledException) { }
            }, _hoverTokenSource.Token);
        }
        
        private void SortServerList()
        {
            if (_isRefreshing) return;
            
            serverListView.SuspendLayout();
            try
            {
                serverListView.Sort(new ServerRowComparer(_serverLookup));
            }
            finally
            {
                serverListView.ResumeLayout();
            }
        }
        
        private class ServerRowComparer : System.Collections.IComparer
        {
            private readonly Dictionary<int, ServerData> _lookup;
            
            public ServerRowComparer(Dictionary<int, ServerData> lookup) => _lookup = lookup;
            
            public int Compare(object? x, object? y)
            {
                if (x is not DataGridViewRow row1 || y is not DataGridViewRow row2) return 0;
                
                bool fav1 = row1.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Value?.ToString() == "⭐";
                bool fav2 = row2.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Value?.ToString() == "⭐";
                
                if (fav1 != fav2) return fav2.CompareTo(fav1);
                
                if (row1.Tag is int id1 && row2.Tag is int id2 &&
                    _lookup.TryGetValue(id1, out var s1) && _lookup.TryGetValue(id2, out var s2))
                {
                    int p1 = Math.Max(0, s1.NumPlayers - s1.BotCount);
                    int p2 = Math.Max(0, s2.NumPlayers - s2.BotCount);
                    return p2.CompareTo(p1);
                }
                return 0;
            }
        }
        
        private MapData? FindBestMapMatch(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || _mapList.Count == 0) return null;
            
            string decodedName = System.Net.WebUtility.HtmlDecode(mapName);
            string normalized = NormalizeMapName(decodedName);
            

            
            // Priority mappings - check these first
            var priorityMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "terroristmansion", "TO-TerrorMansion" },
                { "terrorsmansion", "TO-TerrorMansion" },
                { "terrormansion", "TO-TerrorMansion" },
                { "cia", "TO-CIA" },
                { "glasgowkiss", "TO-GlasgowKiss" },
                { "avalanche", "TO-Avalanche" }
            };
            
            if (priorityMappings.TryGetValue(normalized, out var priorityName))
            {

                
                var match = _mapList.FirstOrDefault(m => m.Name.Equals(priorityName, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
                
                // Try without TO- prefix
                var nameWithoutPrefix = priorityName.StartsWith("TO-") ? priorityName.Substring(3) : priorityName;
                match = _mapList.FirstOrDefault(m => m.Name.EndsWith(nameWithoutPrefix, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
                
                // Try partial match with just the name part
                var namePart = priorityName.Replace("TO-", "").Replace("-", "");
                match = _mapList.FirstOrDefault(m => m.Name.Replace("-", "").Replace("_", "").Contains(namePart, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }
            
            // Direct lookup - normalize both sides
            var directMatch = _mapList.FirstOrDefault(m => NormalizeMapName(m.Name).Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (directMatch != null) return directMatch;
            
            if (normalized.Length < 3) return null;
            
            MapData? bestMatch = null;
            int bestScore = 0;
            int bestLength = int.MaxValue;
            
            foreach (var map in _mapList)
            {
                var mapNormalized = NormalizeMapName(map.Name);
                int score = 0;
                
                if (mapNormalized.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    int lengthPenalty = Math.Abs(map.Name.Length - mapName.Length);
                    score = 1000000 - lengthPenalty * 1000 - map.Name.Length;
                }
                else if (mapNormalized.EndsWith(normalized, StringComparison.OrdinalIgnoreCase) && normalized.Length >= 4)
                {
                    score = 500000 - (mapNormalized.Length - normalized.Length) * 100 - map.Name.Length;
                }
                else if (normalized.EndsWith(mapNormalized, StringComparison.OrdinalIgnoreCase) && mapNormalized.Length >= 4)
                {
                    score = 400000 - (normalized.Length - mapNormalized.Length) * 100 - map.Name.Length;
                }
                else if (mapNormalized.Contains(normalized, StringComparison.OrdinalIgnoreCase) && normalized.Length >= 4)
                {
                    int matchQuality = (normalized.Length * 100) / mapNormalized.Length;
                    score = 300000 + matchQuality * 1000 - map.Name.Length;
                }
                else if (normalized.Contains(mapNormalized, StringComparison.OrdinalIgnoreCase) && mapNormalized.Length >= 4)
                {
                    int matchQuality = (mapNormalized.Length * 100) / normalized.Length;
                    score = 200000 + matchQuality * 1000 - map.Name.Length;
                }
                else if (LevenshteinDistance(mapNormalized, normalized) <= 3 && Math.Abs(mapNormalized.Length - normalized.Length) <= 3)
                {
                    score = 100000 - LevenshteinDistance(mapNormalized, normalized) * 10000 - map.Name.Length;
                }
                else
                {
                    int commonChars = CountCommonSubstring(normalized, mapNormalized);
                    if (commonChars >= Math.Min(normalized.Length, mapNormalized.Length) * 0.6 && commonChars >= 4)
                    {
                        score = commonChars * 1000 - map.Name.Length;
                    }
                }
                
                if (score > bestScore || (score == bestScore && map.Name.Length < bestLength))
                {
                    bestScore = score;
                    bestMatch = map;
                    bestLength = map.Name.Length;
                }
            }
            
            return bestScore > 0 ? bestMatch : null;
        }
        
        private int CountCommonSubstring(string s1, string s2)
        {
            int maxLen = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    int len = 0;
                    while (i + len < s1.Length && j + len < s2.Length && 
                           char.ToLower(s1[i + len]) == char.ToLower(s2[j + len]))
                    {
                        len++;
                    }
                    if (len > maxLen) maxLen = len;
                }
            }
            return maxLen;
        }
        
        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];
            for (int i = 0; i <= s1.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) d[0, j] = j;
            
            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[s1.Length, s2.Length];
        }
        

        
        private string NormalizeMapName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            
            name = System.Net.WebUtility.HtmlDecode(name);
            
            // Remove quotes first
            name = name.Trim('\'', '"').Trim();
            
            // Remove text prefixes BEFORE removing special chars
            if (name.StartsWith("Code-name:", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(10).Trim();
            else if (name.StartsWith("Codename:", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(9).Trim();
            else if (name.StartsWith("2nd -W-", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(7).Trim();
            
            name = name.Replace("'s ", "").Replace("'s", "");
            
            var prefixes = new[] { "TO-", "CTF-", "DM-", "AS-", "=FoE=", "-FoE-", "@8-", "-2-", "-X-", "-x-", "2W-", "SWAT-" };
            foreach (var prefix in prefixes)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(prefix.Length);
                    break;
                }
            }
            
            name = name.Replace("][", "").Replace(" v", "V").Replace(" V", "V")
                      .Replace("{", "").Replace("}", "").Replace("@", "")
                      .Replace("=", "").Replace("+", "").Replace("'", "").Replace("'", "")
                      .Replace("*", "").Replace(":", "").Replace("(", "").Replace(")", "")
                      .Replace(" ", "").Replace("-", "").Replace("_", "").Trim().ToLowerInvariant();
            
            return name;
        }
        
        private void LoadFavorites()
        {
            try
            {
                var favoritesString = Settings.Default.favoriteServers;
                if (!string.IsNullOrEmpty(favoritesString))
                {
                    var favorites = favoritesString.Split(',');
                    foreach (var fav in favorites)
                    {
                        if (!string.IsNullOrWhiteSpace(fav))
                            _favoriteServers.Add(fav.Trim());
                    }
                }
            }
            catch { }
        }
        
        private void SaveFavorites()
        {
            try
            {
                Settings.Default.favoriteServers = string.Join(",", _favoriteServers);
                Settings.Default.Save();
            }
            catch { }
        }
        
        private void ToggleFavorite(string serverKey)
        {
            if (_favoriteServers.Contains(serverKey))
                _favoriteServers.Remove(serverKey);
            else
                _favoriteServers.Add(serverKey);
            SaveFavorites();
        }

        private void serverListView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            CloseMapPreview();
        }
        
        private void CloseMapPreview()
        {
            // Cancel hover and close preview
            _hoverTokenSource?.Cancel();
            _hoverTokenSource?.Dispose();
            _hoverTokenSource = null;

            if (_mapPreviewWindow != null)
            {
                _mapPreviewWindow.Close();
                _mapPreviewWindow.Dispose();
                _mapPreviewWindow = null;
            }
        }

        private async void ShowMapPreview(string imageUrl, string mapName = "", string serverName = "", string currentMap = "")
        {
            if (_mapPreviewWindow != null || _isRefreshing || _isScrolling) return;

            try
            {
                _mapPreviewWindow = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    StartPosition = FormStartPosition.Manual,
                    ShowInTaskbar = false,
                    BackColor = Color.Black,
                    Size = new Size(384, 266),
                    TopMost = true
                };
                
                _mapPreviewWindow.Click += (s, e) => _mapPreviewWindow?.Close();
                
                Point cursor = Cursor.Position;
                _mapPreviewWindow.Location = new Point(cursor.X + 20, cursor.Y + 20);

                // Header panel with server info
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = Color.FromArgb(45, 45, 48),
                    Padding = new Padding(10, 5, 10, 5)
                };

                var serverLabel = new Label
                {
                    Text = serverName,
                    Dock = DockStyle.Top,
                    ForeColor = Color.WhiteSmoke,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = false,
                    Height = 22,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var mapLabel = new Label
                {
                    Text = $"Map: {currentMap}",
                    Dock = DockStyle.Top,
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    AutoSize = false,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                headerPanel.Controls.Add(mapLabel);
                headerPanel.Controls.Add(serverLabel);
                _mapPreviewWindow.Controls.Add(headerPanel);

                if (imageUrl == "NO_PREVIEW")
                {
                    var label = new Label
                    {
                        Text = $"🗺️\n\nNo preview available\nfor this map\n\n{mapName}",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.LightGray,
                        BackColor = Color.FromArgb(45, 45, 48),
                        Font = new Font("Segoe UI", 11, FontStyle.Regular)
                    };
                    label.Click += (s, e) => _mapPreviewWindow?.Close();
                    _mapPreviewWindow.Controls.Add(label);
                    _mapPreviewWindow.Show();
                }
                else if (imageUrl == "LOCAL_IMAGE")
                {
                    var label = new Label
                    {
                        Text = $"MAP PREVIEW\n\n{mapName}\n\nScreenshot would\nbe displayed here",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.WhiteSmoke,
                        BackColor = Color.FromArgb(64, 64, 64),
                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                    };
                    label.Click += (s, e) => _mapPreviewWindow?.Close();
                    _mapPreviewWindow.Controls.Add(label);
                    _mapPreviewWindow.Show();
                }
                else
                {
                    var pictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(32, 32, 32)
                    };
                    pictureBox.Click += (s, e) => _mapPreviewWindow?.Close();
                    _mapPreviewWindow.Controls.Add(pictureBox);
                    _mapPreviewWindow.Show();

                    // Check cache first
                    if (_imageCache.TryGetValue(imageUrl, out var cachedImage))
                    {
                        pictureBox.Image = cachedImage;
                    }
                    else
                    {
                        using var cts = new CancellationTokenSource(5000);
                        using var stream = await _httpClient.GetStreamAsync(imageUrl, cts.Token);
                        var image = Image.FromStream(stream);
                        pictureBox.Image = image;
                        
                        // Cache image (limit cache size)
                        if (_imageCache.Count < 50)
                        {
                            _imageCache[imageUrl] = image;
                        }
                    }
                }
            }
            catch
            {
                _mapPreviewWindow?.Close();
                _mapPreviewWindow?.Dispose();
                _mapPreviewWindow = null;
            }
        }
        


        private void EnableDoubleBuffering(Control control)
        {
            // Enable double buffering
            typeof(Control).InvokeMember("DoubleBuffered", 
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, 
                null, control, new object[] { true });
        }

        private void LoadMasterServersFromSettings()
        {
            string serverInputList = Settings.Default.masterservers;
            if (string.IsNullOrWhiteSpace(serverInputList)) return;

            using var sr = new StringReader(serverInputList);
            while (sr.ReadLine() is string line)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    masterServers.Add(new MasterServer.MasterServerInfo
                    {
                        Address = parts[0],
                        Port = Convert.ToInt16(parts[1])
                    });
                }
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await RefreshServerListAsync();
        }

        // Manual refresh handler
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            Enabled = false; 
            try
            {
                await RefreshServerListAsync();
            }
            finally
            {
                Enabled = true;
            }
        }

        private async Task RefreshServerListAsync()
        {
            if (_isRefreshing) return;
            
            _isRefreshing = true;
            try
            {
                lblDownloadState.Text = "Contacting master servers...";
                serverListView.Rows.Clear();
                servers.Clear();

            var allIPs = new HashSet<string>();
            var downloadTasks = masterServers.Select(ms => MasterServer.DownloadServerListAsync(ms)).ToList();
            var responses = await Task.WhenAll(downloadTasks);

            foreach (var response in responses)
            {
                if (response.errorCode == 0 && response.serverList != null)
                {
                    foreach (var ip in response.serverList) allIPs.Add(ip);
                }
            }

            lblDownloadState.Text = $"{allIPs.Count} servers found. Querying details...";
            
            _serverLookup.Clear();
            var pingTasks = new List<Task>();
            int index = 0;
            foreach (var ip in allIPs)
            {
                int currentIndex = index; 
                pingTasks.Add(QueryServerInfoAndAddToList(currentIndex, ip));
                index++;
            }

            await Task.WhenAll(pingTasks);
            
            // Sort once after all servers are added
            _needsSort = false;
            _sortTimer?.Stop();
            SortServerList();
            
            if (serverListView.Rows.Count == 0 && allIPs.Count > 0)
                lblDownloadState.Text = "Done. No active Tactical Ops servers found.";
            else
                lblDownloadState.Text = $"Done. {serverListView.Rows.Count} servers online.";

                // Auto-select first server
                if (serverListView.Rows.Count > 0)
                {
                    Invoke(() =>
                    {
                        serverListView.Rows[0].Selected = true;
                        serverListView_SelectionChanged(serverListView, EventArgs.Empty);
                    });
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private async Task QueryServerInfoAndAddToList(int id, string ipStr)
        {
            if (!ValidationHelper.IsValidServerAddress(ipStr)) return;
                
            await _pingSemaphore.WaitAsync();
            try
            {
                string[] parts = ipStr.Split(':');
                string ipAddress = parts[0];
                int port = int.Parse(parts[1]);

                ServerData serverData = new ServerData(id, ipStr);
                byte[] data = Encoding.UTF8.GetBytes(@"\info\");

                using (var udp = new UdpClient())
                {
                    udp.Client.SendTimeout = Constants.DEFAULT_UDP_TIMEOUT;
                    udp.Client.ReceiveTimeout = Constants.DEFAULT_UDP_TIMEOUT;
                    udp.Connect(ipAddress, port);

                    Stopwatch sw = Stopwatch.StartNew();
                    await udp.SendAsync(data, data.Length);

                    var receiveTask = udp.ReceiveAsync();
                    var timeoutTask = Task.Delay(Constants.DEFAULT_UDP_TIMEOUT);

                    var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
                    sw.Stop();

                    if (completedTask == receiveTask)
                    {
                        var result = await receiveTask;
                        int ping = Math.Max(1, (int)sw.ElapsedMilliseconds);
                        string response = Encoding.UTF8.GetString(result.Buffer);
                        
                        serverData.SetInfo(response);
                        serverData.Ping = ping; 

                        if (serverData.IsTO220 || serverData.IsTO340 || serverData.IsTO350)
                        {
                            if (serverListView.InvokeRequired)
                            {
                                serverListView.Invoke((System.Windows.Forms.MethodInvoker)delegate
                                {
                                    servers.Add(serverData);
                                    _serverLookup[serverData.Id] = serverData;
                                    AddServerToGrid(serverData);
                                    
                                    if (_isRefreshing)
                                    {
                                        _needsSort = true;
                                        _sortTimer?.Stop();
                                        _sortTimer?.Start();
                                    }
                                });
                            }
                            else
                            {
                                servers.Add(serverData);
                                _serverLookup[serverData.Id] = serverData;
                                AddServerToGrid(serverData);
                                
                                if (_isRefreshing)
                                {
                                    _needsSort = true;
                                    _sortTimer?.Stop();
                                    _sortTimer?.Start();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error querying server {ipStr}: {ex.Message}");
            }
            finally
            {
                _pingSemaphore.Release();
            }
        }

        private void AddServerToGrid(ServerData serverData)
        {
            string name = serverData.ServerName;
            if (serverData.GetProperty("password") == "True")
            {
                name = $"{Constants.ICON_LOCKED} {name}";
            }

            int realPlayers = Math.Max(0, serverData.NumPlayers - serverData.BotCount);
            string totalPlayers = serverData.BotCount > 0
                ? $"{realPlayers} (+{serverData.BotCount} Bots) / {serverData.MaxPlayers}"
                : $"{serverData.NumPlayers} / {serverData.MaxPlayers}";
            
            string serverKey = $"{serverData.ServerIP}:{serverData.ServerPort}";
            bool isFavorite = _favoriteServers.Contains(serverKey);
            
            var newRow = new DataGridViewRow();
            newRow.CreateCells(serverListView, 
                isFavorite ? "⭐" : "☆",
                name,
                serverData.GetProperty("maptitle"),
                totalPlayers,
                serverData.Ping,
                serverData.GetProperty("gametype"));
            newRow.Tag = serverData.Id;
            
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.ForeColor = isFavorite ? Color.Gold : Color.Gray;
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.Font = _starFont;
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            var pingColor = PlayerListRenderer.GetPingColor(serverData.Ping);
            newRow.Cells[UIConstants.PING_COLUMN_INDEX].Style.BackColor = pingColor;
            newRow.Cells[UIConstants.PING_COLUMN_INDEX].Style.SelectionBackColor = pingColor;
            newRow.Cells[UIConstants.PING_COLUMN_INDEX].Style.ForeColor = Color.WhiteSmoke;
            
            serverListView.Rows.Add(newRow);
        }

        private async void serverListView_SelectionChanged(object sender, EventArgs e)
        {
            if (serverListView.SelectedRows.Count == 0 || serverSettingsView?.Parent == null) return;

            var settingsContainer = serverSettingsView.Parent;
            foreach (var panel in settingsContainer.Controls.OfType<FlowLayoutPanel>().ToList())
            {
                settingsContainer.Controls.Remove(panel);
            }
            
            btnJoinServer.BringToFront();
            playerListView.Rows.Clear();
            lblNoPlayers.Hide();
            lblNoResponse.Hide();
            lblWaitingForResponse.Show();

            try
            {
                if (serverListView.SelectedRows[0].Tag is not int index) return;
                if (!_serverLookup.TryGetValue(index, out var selectedServer)) return;

                if (selectedServer != null)
                {
                    selectedServer.ClearPlayerList();
                    await GetServerDetailsAsync(selectedServer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading server details: {ex.Message}");
                lblNoResponse.Show();
                lblWaitingForResponse.Hide();
            }
        }

        private async Task GetServerDetailsAsync(ServerData serverData)
        {
            try
            {
                string ip = serverData.ServerIP;
                int port = serverData.ServerPort;

                using (var udp = new UdpClient())
                {
                    udp.Connect(ip, port);
                    udp.Client.ReceiveTimeout = 2000;

                    byte[] dataStatus = Encoding.UTF8.GetBytes(@"\status\");
                    await udp.SendAsync(dataStatus, dataStatus.Length);
                    
                    var result = await udp.ReceiveAsync();
                    string statusResponse = Encoding.UTF8.GetString(result.Buffer);
                    serverData.UpdateInfo(statusResponse);

                    byte[] dataPlayers = Encoding.UTF8.GetBytes(@"\players\");
                    await udp.SendAsync(dataPlayers, dataPlayers.Length);

                    var receiveTask = udp.ReceiveAsync();
                    if (await Task.WhenAny(receiveTask, Task.Delay(1500)) == receiveTask)
                    {
                         var playerResult = await receiveTask;
                         string playerResponse = Encoding.UTF8.GetString(playerResult.Buffer);
                         serverData.UpdateInfo(playerResponse);
                    }
                    
                    UpdateStatusInfoUI(serverData);
                    UpdateServerListRow(serverData);
                    lblWaitingForResponse.Hide();
                }
            }
            catch
            {
                lblWaitingForResponse.Hide();
                lblNoResponse.Show();
            }
        }

        // Display server settings
        private void UpdateStatusInfoUI(ServerData serverData)
        {
            // Define server settings
            Dictionary<string, string> serverSettings = new Dictionary<string, string>();
            serverSettings["Admin Name"] = serverData.GetProperty("adminname");
            serverSettings["Admin Email"] = serverData.GetProperty("adminemail");
            serverSettings["TOST Version"] = serverData.GetProperty("tostversion");
            serverSettings["ESE Version"] = serverData.GetProperty("protection");
            serverSettings["ESE Mode"] = serverData.GetProperty("esemode");
            serverSettings["Password"] = serverData.GetProperty("password");
            serverSettings["Time Limit"] = serverData.GetProperty("timelimit");
            serverSettings["Min Players"] = serverData.GetProperty("minplayers");
            serverSettings["Friendly Fire"] = serverData.GetProperty("friendlyfire");
            serverSettings["Explosion FF"] = serverData.GetProperty("explositionff");

            // Create settings display panel
            Control? settingsContainer = this.serverSettingsView.Parent;
            if (settingsContainer == null) return;

            var settingsFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5, 5, 5, btnJoinServer.Height + 5),
                BackColor = _themeManager?.GetPanelBackColor() ?? SystemColors.Control
            };
            
            settingsContainer.Controls.Add(settingsFlowPanel);
            settingsFlowPanel.BringToFront();

            using var monoFont = UIConstants.Fonts.MonospaceFont;
            using var headerFont = new Font("Segoe UI", 10, FontStyle.Bold);
            var labelColor = _themeManager?.GetPanelForeColor() ?? SystemColors.ControlText;

            var headerLabel = new Label
            {
                Text = "SERVER SETTINGS:",
                Font = headerFont,
                ForeColor = labelColor,
                Padding = new Padding(0, 0, 0, 8),
                AutoSize = true
            };
            settingsFlowPanel.Controls.Add(headerLabel);

            foreach (var (key, value) in serverSettings)
            {
                var settingLabel = new Label
                {
                    Text = $"{key.PadRight(18, ' ')}: {value}",
                    AutoSize = true,
                    ForeColor = labelColor,
                    Font = monoFont
                };
                settingsFlowPanel.Controls.Add(settingLabel);
            }
            
            // Update player list using renderer
            if (serverData.NumPlayers == 0)
            {
                lblNoPlayers.Show();
            }
            else
            {
                lblNoPlayers.Hide();
                PlayerListRenderer.RenderPlayerList(playerListView, serverData);
                playerListView.Sort(playerListScoreColumn, ListSortDirection.Descending);
                playerListView.Sort(playerListTeamColumn, ListSortDirection.Descending);
            }
        }
        
        private void UpdatePlayerListDisplay(ServerData serverData)
        {
            if (serverData.NumPlayers == 0)
            {
                lblNoPlayers.Show();
                return;
            }
            
            lblNoPlayers.Hide();
            PlayerListRenderer.RenderPlayerList(playerListView, serverData);
            playerListView.Sort(playerListScoreColumn, ListSortDirection.Descending);
            playerListView.Sort(playerListTeamColumn, ListSortDirection.Descending);
        }
        
        private void UpdateServerListRow(ServerData serverData)
        {
            foreach (DataGridViewRow row in serverListView.Rows)
            {
                if (row.Tag is int rowId && rowId == serverData.Id)
                {
                    int realPlayers = Math.Max(0, serverData.NumPlayers - serverData.BotCount);
                    string totalPlayers = serverData.BotCount > 0
                        ? $"{realPlayers} (+{serverData.BotCount} Bots) / {serverData.MaxPlayers}"
                        : $"{serverData.NumPlayers} / {serverData.MaxPlayers}";
                    
                    row.Cells[UIConstants.PLAYERS_COLUMN_INDEX].Value = totalPlayers;
                    break;
                }
            }
        }

        private void btnJoinServer_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is not int index) return;
            
            if (!_serverLookup.TryGetValue(index, out var server)) return;

            string version = server switch
            {
                { IsTO340: true } => "3.4",
                { IsTO350: true } => "3.5",
                { IsTO220: true } => "2.2",
                _ => ""
            };

            if (!string.IsNullOrEmpty(version))
            {
                string serverAddress = $"{server.ServerIP}:{server.GetProperty("hostport")}";
                LaunchGame(version, serverAddress);
                
                if (Settings.Default.closeOnJoin) Close();
            }
        }

        private void LaunchGame(string version, string args = "")
        {
            string settingsPropertyName = version switch
            {
                "2.2" => "to220path",
                "3.4" => "to340path",
                "3.5" => "to350path",
                _ => ""
            };
            
            if (string.IsNullOrEmpty(settingsPropertyName)) return; 

            string? finalPath = Settings.Default[settingsPropertyName] as string;

            if (string.IsNullOrEmpty(finalPath) || !File.Exists(finalPath))
            {
                DialogResult result = MessageBox.Show(
                    $"Tactical Ops {version} executable not found.\n\nLocate the executable file (TacticalOps.exe or UT.exe)?", 
                    $"Locate Tactical Ops {version}", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    using (OpenFileDialog openDialog = new OpenFileDialog())
                    {
                        openDialog.Title = $"Select Tactical Ops {version} Executable (TacticalOps.exe or UT.exe)";
                        openDialog.Filter = "Tactical Ops Executable (*.exe)|TacticalOps.exe;UT.exe|All Files (*.*)|*.*";
                        
                        if (openDialog.ShowDialog() == DialogResult.OK)
                        {
                            finalPath = openDialog.FileName;
                            
                            Settings.Default[settingsPropertyName] = finalPath;
                            Settings.Default.Save();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(finalPath) && File.Exists(finalPath))
            {
                try 
                {
                    Process.Start(finalPath, args);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Could not start game: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        // Event handlers
        
        private void setTacticalOps22PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTO220Dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.to220path = openTO220Dialog.FileName;
                Settings.Default.Save();
            }
        }

        private void setTacticalOps34PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTO340Dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.to340path = openTO340Dialog.FileName;
                Settings.Default.Save();
            }
        }

        private void setTacticalOps35PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTO350Dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.to350path = openTO350Dialog.FileName;
                Settings.Default.Save();
            }
        }
        
        private async void masterserversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormMasterServers msForm = new FormMasterServers())
            {
                msForm.ShowDialog(this); 
            }
            await RefreshServerListAsync();
        }

        private void launchTacticalOps22ToolStripMenuItem_Click(object sender, EventArgs e) => LaunchGame("2.2");
        private void launchTacticalOps34ToolStripMenuItem_Click(object sender, EventArgs e) => LaunchGame("3.4");
        private void launchTacticalOps35ToolStripMenuItem_Click(object sender, EventArgs e) => LaunchGame("3.5");

        private void closeOnJoinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeOnJoinToolStripMenuItem.Checked = !closeOnJoinToolStripMenuItem.Checked;
            Settings.Default.closeOnJoin = closeOnJoinToolStripMenuItem.Checked;
            Settings.Default.Save();
        }
        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(FormAbout dlg = new FormAbout()) 
            { 
                dlg.StartPosition = FormStartPosition.CenterParent; 
                dlg.ShowDialog(this); 
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Data grid event handlers

        private void serverListView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.Button == MouseButtons.Left && e.ColumnIndex == UIConstants.FAVORITES_COLUMN_INDEX)
            {
                if (serverListView.Rows[e.RowIndex].Tag is not int serverId) return;
                if (!_serverLookup.TryGetValue(serverId, out var server)) return;
                
                string serverKey = $"{server.ServerIP}:{server.ServerPort}";
                ToggleFavorite(serverKey);
                
                bool isFavorite = _favoriteServers.Contains(serverKey);
                var cell = serverListView.Rows[e.RowIndex].Cells[UIConstants.FAVORITES_COLUMN_INDEX];
                cell.Value = isFavorite ? "⭐" : "☆";
                cell.Style.ForeColor = isFavorite ? Color.Gold : Color.Gray;
                
                SortServerList();
            }
            else if (e.RowIndex >= 0 && e.Button == MouseButtons.Right)
            {
                serverListView.CurrentCell = serverListView[e.ColumnIndex, e.RowIndex];
                serverListView.Rows[e.RowIndex].Selected = true;
                contextMenuStrip.Show(Cursor.Position);
            }
        }
        


        private void copyIPToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is not int index) return;
            if (!_serverLookup.TryGetValue(index, out var server)) return;
            Clipboard.SetText($"unreal://{server.ServerIP}:{server.GetProperty("hostport")}");
        }

        private void serverListView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn newColumn = serverListView.Columns[e.ColumnIndex];
            ListSortDirection direction;
            if (serverListView.SortedColumn == newColumn && serverListView.SortOrder == SortOrder.Ascending)
                direction = ListSortDirection.Descending;
            else
                direction = ListSortDirection.Ascending;
            serverListView.Sort(newColumn, direction);
        }

        private void serverListView_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
        {
            if (serverListView.Rows[e.RowIndex1].Tag is not int id1 || 
                serverListView.Rows[e.RowIndex2].Tag is not int id2) return;

            if (_serverLookup.TryGetValue(id1, out var s1) && _serverLookup.TryGetValue(id2, out var s2))
            {
                int result = 0;
                if (e.Column.Index == UIConstants.PLAYERS_COLUMN_INDEX)
                {
                    int real1 = Math.Max(0, s1.NumPlayers - s1.BotCount);
                    int real2 = Math.Max(0, s2.NumPlayers - s2.BotCount);
                    
                    result = real1.CompareTo(real2);
                    
                    if (result == 0) result = s1.MaxPlayers.CompareTo(s2.MaxPlayers);
                }
                else if (e.Column.Index == UIConstants.PING_COLUMN_INDEX)
                {
                    result = s1.Ping.CompareTo(s2.Ping);
                }
                else
                {
                   string val1 = e.CellValue1?.ToString() ?? "";
                   string val2 = e.CellValue2?.ToString() ?? "";
                   result = string.Compare(val1, val2);
                }
                e.SortResult = result;
                e.Handled = true;
            }
        }

        private void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _autoRefreshTimer?.Stop();
            _autoRefreshTimer?.Dispose();
            _sortTimer?.Stop();
            _sortTimer?.Dispose();
            _scrollTimer?.Stop();
            _scrollTimer?.Dispose();
            CloseMapPreview();
            
            // Clear image cache
            foreach (var img in _imageCache.Values)
            {
                img?.Dispose();
            }
            _imageCache.Clear();
            
            _pingSemaphore?.Dispose();
            _themeManager?.Dispose();
            _starFont?.Dispose();
            _httpClient?.Dispose();
        }
        
        private void InitializeAutoRefresh()
        {
            int interval = Settings.Default.autoRefreshInterval;
            if (interval > 0 && ValidationHelper.IsValidRefreshInterval(interval))
            {
                _autoRefreshTimer = new System.Windows.Forms.Timer();
                _autoRefreshTimer.Interval = interval * 1000;
                _autoRefreshTimer.Tick += async (s, e) => await RefreshServerListAsync();
                _autoRefreshTimer.Start();
            }
        }
        
        private void serverListView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                btnJoinServer_Click(this, e);
            }
        }
        
        private async void serverListView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                await Task.Delay(50);
                serverListView_SelectionChanged(serverListView, EventArgs.Empty);
            }
        }
        
        private void playerListView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            // Friends list functionality removed
        }

        private void toggleThemeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_themeManager == null) return;
            
            _themeManager.ToggleTheme();
            _themeApplied = false;
            ApplyThemeToControls();
            ConfigureStatusLabels();
            UpdateExistingServerSettings();
            
            Settings.Default.darkMode = _themeManager.IsDarkMode;
            Settings.Default.Save();
        }

        private void UpdateExistingServerSettings()
        {
            if (serverSettingsView?.Parent == null || _themeManager == null) return;
            
            var settingsContainer = serverSettingsView.Parent;
            foreach (var panel in settingsContainer.Controls.OfType<FlowLayoutPanel>())
            {
                panel.BackColor = _themeManager.GetPanelBackColor();
                foreach (Control control in panel.Controls)
                {
                    if (control is Label label)
                    {
                        label.ForeColor = _themeManager.GetPanelForeColor();
                    }
                }
            }
        }
        
        private async void refreshServersToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            await RefreshServerListAsync();
        }
        
        private async void FormMain_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                if (!_isRefreshing)
                {
                    await RefreshServerListAsync();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                this.WindowState = FormWindowState.Minimized;
            }
            else if (e.KeyCode == Keys.Enter && serverListView.SelectedRows.Count > 0)
            {
                e.Handled = true;
                btnJoinServer_Click(this, EventArgs.Empty);
            }
        }
    }
}