#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public class MapPreview : IMapPreview
    {
        private Form? _mapPreviewWindow;
        private CancellationTokenSource? _hoverTokenSource;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, Image> _imageCache;
        private readonly Func<Form> _formFactory;

        private PictureBox? _pictureBox;
        private Label? _noPreviewLabel;
        private Panel? _headerPanel;
        private Label? _serverLabel;
        private Label? _mapLabel;

        public MapPreview(HttpClient httpClient, Dictionary<string, Image> imageCache, Func<Form> formFactory)
        {
            _httpClient = httpClient;
            _imageCache = imageCache;
            _formFactory = formFactory;
        }

        public void ShowMapPreview(string? imageUrl, string mapName, string serverName, string currentMap, System.Drawing.Point cursorPosition)
        {
            _hoverTokenSource?.Cancel();
            _hoverTokenSource = new CancellationTokenSource();
            
            Task.Run(async () => {
                try
                {
                    await Task.Delay(Constants.MAP_PREVIEW_DELAY, _hoverTokenSource.Token);
                    if (!_hoverTokenSource.Token.IsCancellationRequested)
                    {
                        _formFactory().BeginInvoke(new Action(() => ShowMapPreviewWindow(imageUrl, mapName, serverName, currentMap, cursorPosition)));
                    }
                }
                catch (TaskCanceledException) {}
            }, _hoverTokenSource.Token);
        }

        private async void ShowMapPreviewWindow(string? imageUrl, string mapName, string serverName, string currentMap, System.Drawing.Point cursorPosition)
        {
            if (_mapPreviewWindow == null)
            {
                _mapPreviewWindow = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, ShowInTaskbar = false, BackColor = Color.Black, Size = new Size(384, 266), TopMost = true };
                _mapPreviewWindow.Click += (s, e) => CloseMapPreview();
                _mapPreviewWindow.FormClosed += (s, e) => _mapPreviewWindow = null;

                _headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(45, 45, 48), Padding = new Padding(10, 5, 10, 5) };
                _serverLabel = new Label { Dock = DockStyle.Top, ForeColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = false, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
                _mapLabel = new Label { Dock = DockStyle.Top, ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), AutoSize = false, Height = 20, TextAlign = ContentAlignment.MiddleLeft };
                _headerPanel.Controls.AddRange(new Control[] { _mapLabel, _serverLabel });
                _mapPreviewWindow.Controls.Add(_headerPanel);

                _pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(32, 32, 32) };
                _pictureBox.Click += (s, e) => CloseMapPreview();
                _mapPreviewWindow.Controls.Add(_pictureBox);

                _noPreviewLabel = new Label { Text = $"🗺️\n\nNo preview available\nfor this map\n\n", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.LightGray, BackColor = Color.FromArgb(45, 45, 48), Font = new Font("Segoe UI", 11) };
                _noPreviewLabel.Click += (s, e) => CloseMapPreview();
                _mapPreviewWindow.Controls.Add(_noPreviewLabel);

                _headerPanel.BringToFront(); // Ensure header is always on top
            }

            Point desiredLocation = new Point(cursorPosition.X + 20, cursorPosition.Y + 20);
            Rectangle workingArea = Screen.FromControl(_formFactory()).WorkingArea;

            // Adjust X if it goes off-screen to the right
            if (desiredLocation.X + _mapPreviewWindow.Width > workingArea.Right)
            {
                desiredLocation.X = workingArea.Right - _mapPreviewWindow.Width;
            }
            // Adjust Y if it goes off-screen to the bottom
            if (desiredLocation.Y + _mapPreviewWindow.Height > workingArea.Bottom)
            {
                desiredLocation.Y = workingArea.Bottom - _mapPreviewWindow.Height;
            }

            // Ensure it doesn't go off-screen to the left (e.g., if width is larger than screen)
            if (desiredLocation.X < workingArea.Left)
            {
                desiredLocation.X = workingArea.Left;
            }
            // Ensure it doesn't go off-screen to the top
            if (desiredLocation.Y < workingArea.Top)
            {
                desiredLocation.Y = workingArea.Top;
            }

            _mapPreviewWindow.Location = desiredLocation;

            // Update labels
            _serverLabel!.Text = serverName;
            _mapLabel!.Text = $"Map: {currentMap}";

            if (string.IsNullOrEmpty(imageUrl))
            {
                _pictureBox!.Visible = false;
                _noPreviewLabel!.Visible = true;
                _noPreviewLabel.Text = $"🗺️\n\nNo preview available\nfor this map\n\n{mapName}";
                _noPreviewLabel.BringToFront(); // Bring to front since it's the active content
            }
            else
            {
                _noPreviewLabel!.Visible = false;
                _pictureBox!.Visible = true;
                _pictureBox.Image = null; // Clear previous image
                _pictureBox.BringToFront(); // Bring to front since it's the active content

                try
                {
                    if (_imageCache.TryGetValue(imageUrl, out var cachedImage))
                    {
                        _pictureBox.Image = cachedImage;
                    }
                    else
                    {
                        using var cts = new CancellationTokenSource(Constants.MAP_PREVIEW_TIMEOUT);
                        using var stream = await _httpClient.GetStreamAsync(imageUrl, cts.Token);
                        var image = Image.FromStream(stream);
                        if (_imageCache.Count < 50)
                        {
                            _imageCache[imageUrl] = image;
                        }
                        _pictureBox.Image = image;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ShowMapPreview: Image download failed for {imageUrl}: {ex.Message}");
                    _pictureBox.Visible = false;
                    _noPreviewLabel.Visible = true;
                    _noPreviewLabel.Text = $"🗺️\n\nFailed to load preview\n\n{mapName}";
                    _noPreviewLabel.BringToFront();
                }
            }
            _mapPreviewWindow.Show();
        }

        public void CloseMapPreview()
        {
            _hoverTokenSource?.Cancel();
            var windowToClose = _mapPreviewWindow;
            if (windowToClose != null)
            {
                _formFactory().BeginInvoke(new Action(() => windowToClose.Hide())); // Hide instead of Close
            }
            // _mapPreviewWindow is not nullified here, as it's reused.
        }
    }
}
