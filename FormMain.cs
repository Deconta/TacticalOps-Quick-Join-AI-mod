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
        
        private readonly IMapPreviewService _mapPreviewService;
        private readonly ServerListManager _serverListManager;
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
        private readonly IMapService _mapService;
        private ListSortDirection _sortDirection = ListSortDirection.Descending;

        public FormMain()
        {
            _settingsService = new SettingsService();
            _serverProvider = new ServerProvider(_settingsService);
            _mapService = new MapService(_httpClient);
            _mapPreviewService = new MapPreviewService(_httpClient, SynchronizationContext.Current!);
            InitializeComponent();

            this.MinimizeBox = true;
            this.MaximizeBox = false;

            UIHelper.EnableDoubleBuffering(serverListView);
            UIHelper.EnableDoubleBuffering(playerListView);
            UIHelper.EnableDoubleBuffering(serverSettingsView);

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
                _mapPreviewService.CloseMapPreview();
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

            _serverListManager = new ServerListManager(_settingsService, _serverProvider);
            ApplyThemeToControls();
            InitializeAutoRefresh();

            this.KeyPreview = true;
            this.KeyDown += FormMain_KeyDown!;

            serverListView.CellDoubleClick += serverListView_CellDoubleClick!;
            serverListView.CellMouseEnter += serverListView_CellMouseEnter!;
            serverListView.CellMouseLeave += (s, e) => _mapPreviewService.CloseMapPreview();
            serverListView.ColumnHeaderMouseClick += serverListView_ColumnHeaderMouseClick;

            _ = _mapService.LoadMapDataAsync();
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

        private void serverListView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_isRefreshing || _isScrolling || e.RowIndex < 0 || e.ColumnIndex != UIConstants.MAP_COLUMN_INDEX) return;
            if (serverListView.Rows[e.RowIndex].Tag is not int serverId || !_serverListManager.ServerLookup.TryGetValue(serverId, out var server)) return;
            
            string? mapName = serverListView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
            if (string.IsNullOrEmpty(mapName)) return;

            var mapData = _mapService.FindBestMapMatch(mapName);
            string? previewUrl = mapData != null && !string.IsNullOrEmpty(mapData.PreviewSmall) ? mapData.PreviewSmall : mapData?.Preview;

            _mapPreviewService.InitiateMapPreview(previewUrl, mapData?.Name ?? mapName, server.ServerName ?? "Unknown Server", mapName, Cursor.Position);
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
                if (row1.Tag is not int id1 || row2.Tag is not int id2 || !_serverListManager.ServerLookup.TryGetValue(id1, out var s1) || !_serverListManager.ServerLookup.TryGetValue(id2, out var s2)) return 0;
                bool fav1 = _serverListManager.FavoriteServers.Contains($"{s1.ServerIP}:{s1.ServerPort}");
                bool fav2 = _serverListManager.FavoriteServers.Contains($"{s2.ServerIP}:{s2.ServerPort}");
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
                
                await _serverListManager.RefreshServerListAsync(server =>
                {
                    Invoke(new Action(() => AddServerToGrid(server)));
                });
                
                lblDownloadState.Text = $"Done. {serverListView.Rows.Count} servers online.";
                if (serverListView.Rows.Count > 0) Invoke(() => { serverListView.Rows[0].Selected = true; serverListView_SelectionChanged(serverListView, EventArgs.Empty); });
            }
            finally { _isRefreshing = false; }
        }

        private void AddServerToGrid(ServerData serverData)
        {
            var newRow = new DataGridViewRow();
            newRow.CreateCells(serverListView,
                _serverListManager.FavoriteServers.Contains($"{serverData.ServerIP}:{serverData.ServerPort}") ? "★" : "☆",
                serverData.Password ? $"🔒 {serverData.ServerName}" : serverData.ServerName,
                serverData.MapTitle,
                serverData.BotCount > 0 ? $"{Math.Max(0, serverData.NumPlayers - serverData.BotCount)} (+{serverData.BotCount} Bots) / {serverData.MaxPlayers}" : $"{serverData.NumPlayers} / {serverData.MaxPlayers}",
                serverData.Ping, serverData.GameType);
            newRow.Tag = serverData.Id;
            bool isFavorite = _serverListManager.FavoriteServers.Contains($"{serverData.ServerIP}:{serverData.ServerPort}");
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

        private void UpdateServerGridRow(DataGridViewRow row, ServerData serverData)
        {
            // Update the cells of the given row
            row.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Value = _serverListManager.FavoriteServers.Contains($"{serverData.ServerIP}:{serverData.ServerPort}") ? "★" : "☆";
            row.Cells[1].Value = serverData.Password ? $"🔒 {serverData.ServerName}" : serverData.ServerName;
            row.Cells[UIConstants.MAP_COLUMN_INDEX].Value = serverData.MapTitle;
            row.Cells[UIConstants.PLAYERS_COLUMN_INDEX].Value = serverData.BotCount > 0 ? $"{Math.Max(0, serverData.NumPlayers - serverData.BotCount)} (+{serverData.BotCount} Bots) / {serverData.MaxPlayers}" : $"{serverData.NumPlayers} / {serverData.MaxPlayers}";
            row.Cells[UIConstants.PING_COLUMN_INDEX].Value = serverData.Ping;
            row.Cells[UIConstants.VERSION_COLUMN_INDEX].Value = serverData.GameType;

            // Update styles
            bool isFavorite = _serverListManager.FavoriteServers.Contains($"{serverData.ServerIP}:{serverData.ServerPort}");
            row.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.ForeColor = isFavorite ? Color.Yellow : Color.Gray;
            row.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.SelectionForeColor = isFavorite ? Color.Gold : Color.Gray;
            row.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.Font = _starFont;
            row.Cells[UIConstants.FAVORITES_COLUMN_INDEX].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            var pingColor = PlayerListRenderer.GetPingColor(serverData.Ping);
            row.Cells[UIConstants.PING_COLUMN_INDEX].Style.BackColor = pingColor;
            row.Cells[UIConstants.PING_COLUMN_INDEX].Style.SelectionBackColor = pingColor;
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
                if (serverListView.SelectedRows[0].Tag is int index && _serverListManager.ServerLookup.TryGetValue(index, out var server))
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
                ["Admin Name"] = serverData.GetProperty("adminname") ?? "N/A", 
                ["Admin Email"] = serverData.GetProperty("adminemail") ?? "N/A", 
                ["TOST Version"] = serverData.GetProperty("tostversion") ?? "N/A", 
                ["ESE Version"] = serverData.GetProperty("protection") ?? "N/A", 
                ["ESE Mode"] = serverData.GetProperty("esemode") ?? "N/A", 
                ["Password"] = serverData.Password.ToString(), 
                ["Time Limit"] = serverData.GetProperty("timelimit") ?? "N/A", 
                ["Min Players"] = serverData.GetProperty("minplayers") ?? "N/A", 
                ["Friendly Fire"] = serverData.GetProperty("friendlyfire") ?? "N/A", 
                ["Explosion FF"] = serverData.GetProperty("explositionff") ?? "N/A"
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
            
            // Populate Player List
            playerListView.Rows.Clear();
            serverData.Players.Clear(); // Clear existing players before re-populating

            int numPlayers = 0;
            if (int.TryParse(serverData.GetProperty("numplayers"), out int parsedNumPlayers))
            {
                numPlayers = parsedNumPlayers;
            }

            if (numPlayers == 0)
            {
                lblNoPlayers.Show();
            }
            else
            {
                lblNoPlayers.Hide();
                for (int i = 0; i < numPlayers; i++)
                {
                    string playerName = serverData.GetProperty("player_" + i.ToString());
                    if (!string.IsNullOrEmpty(playerName))
                    {
                        int score = Convert.ToInt32(serverData.GetProperty("score_" + i.ToString()));
                        int kills = Convert.ToInt32(serverData.GetProperty("frags_" + i.ToString()));
                        int deaths = Convert.ToInt32(serverData.GetProperty("deaths_" + i.ToString()));
                        int ping = Convert.ToInt32(serverData.GetProperty("ping_" + i.ToString()));
                        int team = Convert.ToInt32(serverData.GetProperty("team_" + i.ToString()));

                        var player = new Player { 
                            Id = i, 
                            Name = playerName, 
                            Score = score, 
                            Kills = kills, 
                            Deaths = deaths, 
                            Ping = ping, 
                            Team = team 
                        };
                        serverData.Players.Add(player);
                    }
                }
                PlayerListRenderer.RenderPlayerList(playerListView, serverData); // Render all players at once
                playerListView.Sort(playerListScoreColumn, ListSortDirection.Descending);
                playerListView.Sort(playerListTeamColumn, ListSortDirection.Descending);
            }
        }

        private void UpdateServerListRow(ServerData serverData)
        {
            var row = serverListView.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag is int id && id == serverData.Id);
            if (row == null) return;
            UpdateServerGridRow(row, serverData);
        }

        private void btnJoinServer_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is not int index || !_serverListManager.ServerLookup.TryGetValue(index, out var server)) return;
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
                if (serverListView.Rows[e.RowIndex].Tag is int id && _serverListManager.ServerLookup.TryGetValue(id, out var s))
                {
                    string key = $"{s.ServerIP}:{s.ServerPort}";
                    _serverListManager.ToggleFavorite(key);
                    var cell = serverListView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    bool isFav = _serverListManager.FavoriteServers.Contains(key);
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
            if (serverListView.CurrentRow?.Tag is int index && _serverListManager.ServerLookup.TryGetValue(index, out var server))
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
            _mapPreviewService.CloseMapPreview();
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

        private async void refreshServersToolStripMenuItem_Click(object? sender, EventArgs e) => await RefreshServerListAsync();
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int index && _serverListManager.ServerLookup.TryGetValue(index, out var server))
                addToFavoritesToolStripMenuItem.Text = _serverListManager.FavoriteServers.Contains($"{server.ServerIP}:{server.ServerPort}") ? "Von Favoriten entfernen" : "Zu Favoriten hinzufügen";
        }

        private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int serverId && _serverListManager.ServerLookup.TryGetValue(serverId, out var server))
            {
                string key = $"{server.ServerIP}:{server.ServerPort}";
                _serverListManager.ToggleFavorite(key);
                var cell = serverListView.CurrentRow.Cells[UIConstants.FAVORITES_COLUMN_INDEX];
                bool isFav = _serverListManager.FavoriteServers.Contains(key);
                cell.Value = isFav ? "★" : "☆";
                cell.Style.ForeColor = isFav ? Color.Yellow : Color.Gray;
                SortServerList();
            }
        }
        private async void refreshServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serverListView.CurrentRow?.Tag is int index && _serverListManager.ServerLookup.TryGetValue(index, out var server))
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