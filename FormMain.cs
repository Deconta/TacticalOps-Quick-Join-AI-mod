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

namespace TacticalOpsQuickJoin
{
    public partial class FormMain : Form
    {
        private readonly HttpClient _httpClient = new();

        private List<ServerData> servers = [];
        private readonly Dictionary<int, ServerData> _serverLookup = new();
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
        private DataGridViewColumn? _sortedColumn;
        private readonly ISettingsService _settingsService;
        private readonly IServerProvider _serverProvider;
        private ListSortDirection _sortDirection = ListSortDirection.Descending;

        public FormMain()
        {
            _settingsService = new SettingsService();
            _serverProvider = new ServerProvider(_settingsService);
            InitializeComponent();

            this.MinimizeBox = true;
            this.MaximizeBox = false;

            EnableDoubleBuffering(serverListView);
            EnableDoubleBuffering(playerListView);
            EnableDoubleBuffering(serverSettingsView);

            serverListView.RowHeadersVisible = false;
            serverListView.AllowUserToResizeRows = false;
			
            // Use a timer to sort the list periodically during refresh, giving a "live" feel
            _sortTimer = new System.Windows.Forms.Timer();
            _sortTimer.Interval = 500; // Reduced interval for more frequent updates
            _sortTimer.Tick += (s, e) =>
            {
                _sortTimer?.Stop(); // Stop after firing
                if (_needsSort)
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

            _themeManager = new ThemeManager(this, _settingsService.DarkMode);
            if (Controls.Find("menuStrip1", true).FirstOrDefault() is MenuStrip menuStrip)
            {
                _themeManager.ApplyToMenuStrip(menuStrip);
            }

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
            serverListView.CellMouseEnter += serverListView_CellMouseEnter!;
            serverListView.CellMouseLeave += serverListView_CellMouseLeave!;
            serverListView.ColumnHeaderMouseClick += serverListView_ColumnHeaderMouseClick;

            _ = LoadMapDataAsync();
            ConfigureColumnWidths();

            playerListScoreColumn.ValueType = typeof(Int32);
            playerListKillsColumn.ValueType = typeof(Int32);
            playerListDeathColumn.ValueType = typeof(Int32);
            playerListPingColumn.ValueType = typeof(Int32);
            playerListTeamColumn.ValueType = typeof(Int32);

            closeOnJoinToolStripMenuItem.Checked = _settingsService.CloseOnJoin;
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
                serverListView.Columns[UIConstants.VERSION_COLUMN_INDEX].Width = 60;

                var centerAlignedCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter };
                serverListView.Columns[UIConstants.MAP_COLUMN_INDEX].DefaultCellStyle = centerAlignedCellStyle;
                serverListView.Columns[UIConstants.PLAYERS_COLUMN_INDEX].DefaultCellStyle = centerAlignedCellStyle;
                serverListView.Columns[UIConstants.PING_COLUMN_INDEX].DefaultCellStyle = centerAlignedCellStyle;
                serverListView.Columns[UIConstants.VERSION_COLUMN_INDEX].DefaultCellStyle = centerAlignedCellStyle;
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
                _themeManager.ApplyToMenuStrip(menuStrip);
            _themeApplied = true;
        }

        private async Task LoadMapDataAsync()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataFolder, "TacticalOpsQuickJoin");
            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }
            string localJsonPath = Path.Combine(appSpecificFolder, "maps.json");

            try
            {
                string jsonString;
                if (File.Exists(localJsonPath))
                {
                    jsonString = await File.ReadAllTextAsync(localJsonPath);
                }
                else
                {
                    jsonString = await _httpClient.GetStringAsync(Constants.MAP_JSON_URL);
                    await File.WriteAllTextAsync(localJsonPath, jsonString);
                }
                
                _mapList = JsonSerializer.Deserialize<List<MapData>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                BuildMapLookup();
            }
            catch { /* Gulp */ }
        }

        private void BuildMapLookup()
        {
            _mapLookup.Clear();
            foreach (var map in _mapList)
            {
                var normalized = NormalizeMapName(map.Name);
                if (!_mapLookup.ContainsKey(normalized))
                    _mapLookup[normalized] = map;
            }
        }

        private void serverListView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_isRefreshing || _isScrolling || e.RowIndex < 0 || e.ColumnIndex != UIConstants.MAP_COLUMN_INDEX) return;
            if (serverListView.Rows[e.RowIndex].Tag is not int serverId || !_serverLookup.TryGetValue(serverId, out var server)) return;
            
            string? mapName = serverListView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
            if (string.IsNullOrEmpty(mapName)) return;

            var mapData = FindBestMapMatch(mapName);
            string? previewUrl = mapData != null && !string.IsNullOrEmpty(mapData.PreviewSmall) ? mapData.PreviewSmall : mapData?.Preview;

            _hoverTokenSource?.Cancel();
            _hoverTokenSource = new CancellationTokenSource();
            
            Task.Run(async () => {
                try
                {
                    await Task.Delay(Constants.MAP_PREVIEW_DELAY, _hoverTokenSource.Token);
                    if (!_hoverTokenSource.Token.IsCancellationRequested)
                        this.BeginInvoke(() => ShowMapPreview(previewUrl, mapData?.Name ?? mapName, server.ServerName, mapName));
                }
                catch (TaskCanceledException) {}
            }, _hoverTokenSource.Token);
        }

        private void SortServerList()
        {
            if (serverListView.Rows.Count == 0) return;
            _sortTimer?.Stop();
            _needsSort = false;
            serverListView.SuspendLayout();
            _sortedColumn ??= serverListView.Columns[UIConstants.PLAYERS_COLUMN_INDEX];
            var rows = new List<DataGridViewRow>(serverListView.Rows.Cast<DataGridViewRow>());
            rows.Sort((row1, row2) =>
            {
                if (row1.Tag is not int id1 || row2.Tag is not int id2 || !_serverLookup.TryGetValue(id1, out var s1) || !_serverLookup.TryGetValue(id2, out var s2)) return 0;
                bool fav1 = _favoriteServers.Contains($"{s1.ServerIP}:{s1.ServerPort}");
                bool fav2 = _favoriteServers.Contains($"{s2.ServerIP}:{s2.ServerPort}");
                if (fav1 != fav2) return fav2.CompareTo(fav1);
                int result = 0;
                int colIndex = _sortedColumn!.Index;
                if (colIndex == UIConstants.PLAYERS_COLUMN_INDEX)
                {
                    int p1 = Math.Max(0, s1.NumPlayers - s1.BotCount);
                    int p2 = Math.Max(0, s2.NumPlayers - s2.BotCount);
                    result = p1.CompareTo(p2);
                    if (result == 0) result = s1.MaxPlayers.CompareTo(s2.MaxPlayers);
                }
                else if (colIndex == UIConstants.PING_COLUMN_INDEX) result = s1.Ping.CompareTo(s2.Ping);
                else if (colIndex != UIConstants.FAVORITES_COLUMN_INDEX)
                {
                    result = string.Compare(row1.Cells[colIndex].Value?.ToString(), row2.Cells[colIndex].Value?.ToString(), StringComparison.OrdinalIgnoreCase);
                }
                return _sortDirection == ListSortDirection.Descending ? -result : result;
            });
            serverListView.Rows.Clear();
            serverListView.Rows.AddRange(rows.ToArray());
            foreach (DataGridViewColumn column in serverListView.Columns)
                column.HeaderCell.SortGlyphDirection = (column == _sortedColumn) ? (_sortDirection == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending) : SortOrder.None;
            serverListView.ResumeLayout();
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
                var favorites = (_settingsService.FavoriteServers ?? "").Split(',');
                foreach (var fav in favorites.Where(f => !string.IsNullOrWhiteSpace(f)))
                    _favoriteServers.Add(fav.Trim());
            }
            catch { }
        }

        private void SaveFavorites()
        {
            try
            {
                _settingsService.FavoriteServers = string.Join(",", _favoriteServers);
                _settingsService.Save();
            }
            catch { }
        }

        private void ToggleFavorite(string serverKey)
        {
            if (!_favoriteServers.Remove(serverKey))
                _favoriteServers.Add(serverKey);
            SaveFavorites();
        }

        private void serverListView_CellMouseLeave(object sender, DataGridViewCellEventArgs e) => CloseMapPreview();

        private void CloseMapPreview()
        {
            _hoverTokenSource?.Cancel();
            _mapPreviewWindow?.Close();
            _mapPreviewWindow = null;
        }

        private async void ShowMapPreview(string? imageUrl, string mapName, string serverName, string currentMap)
        {
            if (_mapPreviewWindow != null) return;
            // Defensive null check for _httpClient, though it should be initialized.
            if (_httpClient == null)
            {
                Debug.WriteLine("Error: _httpClient is null in ShowMapPreview.");
                return;
            }

            Form newPreviewWindow = null; // Create locally first
            try
            {
                newPreviewWindow = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, ShowInTaskbar = false, BackColor = Color.Black, Size = new Size(384, 266), TopMost = true };
                newPreviewWindow.Click += (s, e) => newPreviewWindow?.Close();
                newPreviewWindow.Location = new Point(Cursor.Position.X + 20, Cursor.Position.Y + 20);

                var headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(45, 45, 48), Padding = new Padding(10, 5, 10, 5) };
                var serverLabel = new Label { Text = serverName, Dock = DockStyle.Top, ForeColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = false, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
                var mapLabel = new Label { Text = $"Map: {currentMap}", Dock = DockStyle.Top, ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), AutoSize = false, Height = 20, TextAlign = ContentAlignment.MiddleLeft };
                headerPanel.Controls.AddRange(new Control[] { mapLabel, serverLabel });
                newPreviewWindow.Controls.Add(headerPanel);

                if (string.IsNullOrEmpty(imageUrl)) { 
                     var label = new Label { Text = $"🗺️\n\nNo preview available\nfor this map\n\n{mapName}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.LightGray, BackColor = Color.FromArgb(45, 45, 48), Font = new Font("Segoe UI", 11) };
                    newPreviewWindow.Controls.Add(label);
                } else {
                    var pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(32, 32, 32) };
                    pictureBox.Click += (s, e) => newPreviewWindow?.Close();
                    newPreviewWindow.Controls.Add(pictureBox);

                    try {
                        Debug.WriteLine($"ShowMapPreview: Attempting to download image from URL: {imageUrl}");
                        if (_imageCache.TryGetValue(imageUrl, out var cachedImage)) pictureBox.Image = cachedImage;
                        else {
                            // Defensive check: _httpClient could be null here if it somehow got nulled or disposed after the initial check.
                            if (_httpClient == null)
                            {
                                Debug.WriteLine("Error: _httpClient is unexpectedly null just before GetStreamAsync.");
                                newPreviewWindow?.Close(); // Close the window to prevent further issues.
                                return;
                            }
                            using var cts = new CancellationTokenSource(5000);
                            using var stream = await _httpClient.GetStreamAsync(imageUrl, cts.Token); 
                            var image = Image.FromStream(stream);
                            pictureBox.Image = image;
                            if (_imageCache.Count < 50) _imageCache[imageUrl] = image;
                        }
                    } catch (Exception ex) { 
                        Debug.WriteLine($"ShowMapPreview: Image download failed for {imageUrl}: {ex.Message}");
                        // If image download fails, show a "No preview" message instead of crashing
                        var label = new Label { Text = $"🗺️\n\nFailed to load preview\n\n{mapName}", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.LightGray, BackColor = Color.FromArgb(45, 45, 48), Font = new Font("Segoe UI", 11) };
                        newPreviewWindow.Controls.Remove(pictureBox); // Remove the broken picture box
                        newPreviewWindow.Controls.Add(label);
                    }
                }
                
                _mapPreviewWindow = newPreviewWindow; // Assign to class field ONLY IF successful
                _mapPreviewWindow.Show(); // Now show the window
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowMapPreview: General error during window setup: {ex.Message}");
                newPreviewWindow?.Close(); // Close if it was created but not assigned to _mapPreviewWindow
                newPreviewWindow = null;
            }
        }

        private async void Form1_Load(object sender, EventArgs e) => await RefreshServerListAsync();
        private async void btnRefresh_Click(object sender, EventArgs e) => await RefreshServerListAsync();

        private async Task RefreshServerListAsync()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;
            try
            {
                lblDownloadState.Text = "Contacting master servers...";
                serverListView.Rows.Clear();
                servers.Clear();
                _serverLookup.Clear();
                
                await _serverProvider.GetServerListAsync(server =>
                {
                    Invoke(new Action(() => AddServer(server)));
                });
                
                lblDownloadState.Text = $"Done. {serverListView.Rows.Count} servers online.";
                if (serverListView.Rows.Count > 0) Invoke(() => { serverListView.Rows[0].Selected = true; serverListView_SelectionChanged(serverListView, EventArgs.Empty); });
            }
            finally { _isRefreshing = false; }
        }

        private void AddServer(ServerData serverData)
        {
            servers.Add(serverData);
            _serverLookup[serverData.Id] = serverData;
            AddServerToGrid(serverData);
        }

        private void AddServerToGrid(ServerData serverData)
        {
            var newRow = new DataGridViewRow();
            newRow.CreateCells(serverListView,
                _favoriteServers.Contains($"{serverData.ServerIP}:{serverData.ServerPort}") ? "★" : "☆",
                serverData.Password ? $"🔒 {serverData.ServerName}" : serverData.ServerName,
                serverData.MapTitle,
                serverData.BotCount > 0 ? $"{Math.Max(0, serverData.NumPlayers - serverData.BotCount)} (+{serverData.BotCount} Bots) / {serverData.MaxPlayers}" : $"{serverData.NumPlayers} / {serverData.MaxPlayers}",
                serverData.Ping, serverData.GameType);
            newRow.Tag = serverData.Id;
            bool isFavorite = _favoriteServers.Contains($"{serverData.ServerIP}:{serverData.ServerPort}");
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.ForeColor = isFavorite ? Color.Yellow : Color.Gray;
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.SelectionForeColor = isFavorite ? Color.Gold : Color.Gray; 
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.Font = _starFont;
            newRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            var pingColor = PlayerListRenderer.GetPingColor(serverData.Ping);
            newRow.Cells[UIConstants.PING_COLUMN_INDEX].Style.BackColor = pingColor;
            newRow.Cells[UIConstants.PING_COLUMN_INDEX].Style.SelectionBackColor = pingColor;
            serverListView.Rows.Add(newRow);
            _needsSort = true;
            if (_sortTimer != null && !_sortTimer.Enabled)
            {
                _sortTimer.Start();
            }
        }

        private async void serverListView_SelectionChanged(object sender, EventArgs e)
        {
            if (serverListView.SelectedRows.Count == 0 || serverSettingsView?.Parent == null) return;
            serverSettingsView.Parent.Controls.OfType<FlowLayoutPanel>().ToList().ForEach(p => serverSettingsView.Parent.Controls.Remove(p));
            btnJoinServer.BringToFront();
            playerListView.Rows.Clear();
            lblNoPlayers.Hide();
            lblNoResponse.Hide();
            lblWaitingForResponse.Show();
            try
            {
                if (serverListView.SelectedRows[0].Tag is int index && _serverLookup.TryGetValue(index, out var server))
                {
                    server.ClearPlayerList();
                    await _serverProvider.GetServerDetailsAsync(server);
                    UpdateStatusInfoUI(server);
                    UpdateServerListRow(server);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Error loading details: {ex.Message}"); lblNoResponse.Show(); lblWaitingForResponse.Hide(); }
            finally { lblWaitingForResponse.Hide(); }
        }
        
        private void UpdateStatusInfoUI(ServerData serverData)
        {
            var settings = new Dictionary<string, string> { 
                ["Admin Name"] = serverData.AdminName, 
                ["Admin Email"] = serverData.AdminEmail, 
                ["TOST Version"] = serverData.TostVersion, 
                ["ESE Version"] = serverData.Protection, 
                ["ESE Mode"] = serverData.EseMode, 
                ["Password"] = serverData.Password.ToString(), 
                ["Time Limit"] = serverData.TimeLimit, 
                ["Min Players"] = serverData.MinPlayers, 
                ["Friendly Fire"] = serverData.FriendlyFire, 
                ["Explosion FF"] = serverData.ExplosionFF
            };
            if (serverSettingsView.Parent is not Control settingsContainer) return;
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(5, 5, 5, btnJoinServer.Height + 5), BackColor = _themeManager?.GetPanelBackColor() ?? SystemColors.Control };
            settingsContainer.Controls.Add(panel);
            panel.BringToFront();
            using var monoFont = UIConstants.Fonts.MonospaceFont;
            using var headerFont = new Font("Segoe UI", 10, FontStyle.Bold);
            var labelColor = _themeManager?.GetPanelForeColor() ?? SystemColors.ControlText;
            panel.Controls.Add(new Label { Text = "SERVER SETTINGS:", Font = headerFont, ForeColor = labelColor, Padding = new Padding(0, 0, 0, 8), AutoSize = true });
            foreach (var (key, value) in settings)
                panel.Controls.Add(new Label { Text = $"{key.PadRight(18)}: {value}", AutoSize = true, ForeColor = labelColor, Font = monoFont });
            
            if (serverData.Players.Count == 0) lblNoPlayers.Show();
            else
            {
                lblNoPlayers.Hide();
                PlayerListRenderer.RenderPlayerList(playerListView, serverData);
                playerListView.Sort(playerListScoreColumn, ListSortDirection.Descending);
                playerListView.Sort(playerListTeamColumn, ListSortDirection.Descending);
            }
        }

        private void UpdateServerListRow(ServerData serverData)
        {
            var row = serverListView.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag is int id && id == serverData.Id);
            if (row == null) return;
            row.Cells[UIConstants.MAP_COLUMN_INDEX].Value = serverData.MapTitle;
            row.Cells[UIConstants.PLAYERS_COLUMN_INDEX].Value = serverData.BotCount > 0 ? $"{Math.Max(0, serverData.NumPlayers - serverData.BotCount)} (+{serverData.BotCount} Bots) / {serverData.MaxPlayers}" : $"{serverData.NumPlayers} / {serverData.MaxPlayers}";
            row.Cells[UIConstants.PING_COLUMN_INDEX].Value = serverData.Ping;
        }

        private void btnJoinServer_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is not int index || !_serverLookup.TryGetValue(index, out var server)) return;
            string version = server.IsTO340 ? "3.4" : server.IsTO350 ? "3.5" : server.IsTO220 ? "2.2" : "";
            if (!string.IsNullOrEmpty(version))
            {
                GameLauncher.LaunchGame(version, _settingsService, $"{server.ServerIP}:{server.HostPort}");
                if (_settingsService.CloseOnJoin) Close();
            }
        }
        
        private void setTacticalOps22PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTO220Dialog.ShowDialog() == DialogResult.OK)
            {
                _settingsService.SetGamePath("2.2", openTO220Dialog.FileName!);
                _settingsService.Save();
            }
        }

        private void setTacticalOps34PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTO340Dialog.ShowDialog() == DialogResult.OK)
            {
                _settingsService.SetGamePath("3.4", openTO340Dialog.FileName!);
                _settingsService.Save();
            }
        }

        private void setTacticalOps35PathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTO350Dialog.ShowDialog() == DialogResult.OK)
            {
                _settingsService.SetGamePath("3.5", openTO350Dialog.FileName!);
                _settingsService.Save();
            }
        }

        private async void masterserversToolStripMenuItem_Click(object sender, EventArgs e) { using (var f = new FormMasterServers()) f.ShowDialog(this); await RefreshServerListAsync(); }
        private void launchTacticalOps22ToolStripMenuItem_Click(object sender, EventArgs e) => GameLauncher.LaunchGame("2.2", _settingsService);
        private void launchTacticalOps34ToolStripMenuItem_Click(object sender, EventArgs e) => GameLauncher.LaunchGame("3.4", _settingsService);
        private void launchTacticalOps35ToolStripMenuItem_Click(object sender, EventArgs e) => GameLauncher.LaunchGame("3.5", _settingsService);
        private void closeOnJoinToolStripMenuItem_Click(object sender, EventArgs e) { _settingsService.CloseOnJoin = !_settingsService.CloseOnJoin; _settingsService.Save(); }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) { using(var dlg = new FormAbout()) dlg.ShowDialog(this); }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void serverListView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.Button == MouseButtons.Left && e.ColumnIndex == UIConstants.FAVORITES_COLUMN_INDEX)
            {
                if (serverListView.Rows[e.RowIndex].Tag is int id && _serverLookup.TryGetValue(id, out var s))
                {
                    string key = $"{s.ServerIP}:{s.ServerPort}";
                    ToggleFavorite(key);
                    var cell = serverListView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    bool isFav = _favoriteServers.Contains(key);
                    cell.Value = isFav ? "★" : "☆";
                    cell.Style.ForeColor = isFav ? Color.Yellow : Color.Gray;
                    SortServerList();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                serverListView.CurrentCell = serverListView[e.ColumnIndex, e.RowIndex];
                serverListView.Rows[e.RowIndex].Selected = true;
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void copyIPToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int index && _serverLookup.TryGetValue(index, out var server))
                Clipboard.SetText($"unreal://{server.ServerIP}:{server.HostPort}");
        }

        private void serverListView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            var newColumn = serverListView.Columns[e.ColumnIndex];
            _sortDirection = (_sortedColumn == newColumn && _sortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
            _sortedColumn = newColumn;
            SortServerList();
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
            foreach (var img in _imageCache.Values) img?.Dispose();
            _themeManager?.Dispose();
            _starFont?.Dispose();
            _httpClient?.Dispose();
        }

        private void InitializeAutoRefresh()
        {
            int interval = _settingsService.AutoRefreshInterval;
            if (interval > 0 && ValidationHelper.IsValidRefreshInterval(interval))
            {
                _autoRefreshTimer = new System.Windows.Forms.Timer { Interval = interval * 1000 };
                _autoRefreshTimer.Tick += async (s, e) => await RefreshServerListAsync();
                _autoRefreshTimer.Start();
            }
        }

        private void serverListView_CellDoubleClick(object sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) btnJoinServer_Click(this, e); }
        private async void serverListView_CellClick(object? sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) { await Task.Delay(50); serverListView_SelectionChanged(serverListView, EventArgs.Empty); } }
        private void toggleThemeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_themeManager == null) return;
            _themeManager.ToggleTheme();
            _themeApplied = false;
            ApplyThemeToControls();
            ConfigureStatusLabels();
            UpdateExistingServerSettings();
            _settingsService.DarkMode = _themeManager.IsDarkMode;
            _settingsService.Save();
        }

        private void UpdateExistingServerSettings()
        {
            if (serverSettingsView?.Parent == null || _themeManager == null) return;
            var settingsContainer = serverSettingsView.Parent;
            foreach (var panel in settingsContainer.Controls.OfType<FlowLayoutPanel>())
            {
                panel.BackColor = _themeManager.GetPanelBackColor();
                foreach (Label label in panel.Controls.OfType<Label>()) label.ForeColor = _themeManager.GetPanelForeColor();
            }
        }

        private void EnableDoubleBuffering(Control control) => typeof(Control).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, control, new object[] { true });

        private async void refreshServersToolStripMenuItem_Click(object? sender, EventArgs e) => await RefreshServerListAsync();
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int index && _serverLookup.TryGetValue(index, out var server))
                addToFavoritesToolStripMenuItem.Text = _favoriteServers.Contains($"{server.ServerIP}:{server.ServerPort}") ? "Von Favoriten entfernen" : "Zu Favoriten hinzufügen";
        }

        private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int serverId && _serverLookup.TryGetValue(serverId, out var server))
            {
                string key = $"{server.ServerIP}:{server.ServerPort}";
                ToggleFavorite(key);
                var cell = serverListView.CurrentRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX];
                bool isFav = _favoriteServers.Contains(key);
                cell.Value = isFav ? "★" : "☆";
                cell.Style.ForeColor = isFav ? Color.Yellow : Color.Gray;
                SortServerList();
            }
        }

        private async void refreshServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int index && _serverLookup.TryGetValue(index, out var server))
                await RefreshSingleServerAsync(server);
        }

        private async Task RefreshSingleServerAsync(ServerData serverData)
        {
            try 
            { 
                await _serverProvider.GetServerDetailsAsync(serverData);
                UpdateStatusInfoUI(serverData);
                UpdateServerListRow(serverData);
            }
            catch (Exception ex) { Debug.WriteLine($"Error refreshing single server: {ex.Message}"); }
        }

        private async void FormMain_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) { e.Handled = true; if (!_isRefreshing) await RefreshServerListAsync(); }
            else if (e.KeyCode == Keys.Escape) { e.Handled = true; this.WindowState = FormWindowState.Minimized; }
            else if (e.KeyCode == Keys.Enter && serverListView.SelectedRows.Count > 0) { e.Handled = true; btnJoinServer_Click(this, EventArgs.Empty); }
        }
    }
}
