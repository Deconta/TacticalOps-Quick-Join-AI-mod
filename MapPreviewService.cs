#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public class MapPreviewService : IMapPreviewService
    {
        private readonly HttpClient _httpClient;
        private readonly SynchronizationContext _syncContext;
        private Form? _mapPreviewWindow;
        private CancellationTokenSource? _hoverTokenSource;
        private readonly Dictionary<string, Image> _imageCache;

        public MapPreviewService(HttpClient httpClient, SynchronizationContext syncContext)
        {
            _httpClient = httpClient;
            _syncContext = syncContext;
            _imageCache = new Dictionary<string, Image>();
        }

        public void InitiateMapPreview(string? imageUrl, string mapName, string serverName, string currentMap, Point cursorPosition)
        {
            _hoverTokenSource?.Cancel(); // Cancel any existing delayed previews
            _hoverTokenSource = new CancellationTokenSource();
            
            Task.Run(async () => {
                try
                {
                    await Task.Delay(Constants.MAP_PREVIEW_DELAY, _hoverTokenSource.Token);
                    if (!_hoverTokenSource.Token.IsCancellationRequested)
                    {
                        _syncContext.Post(_ => ShowMapPreviewInternal(imageUrl, mapName, serverName, currentMap, cursorPosition), null);
                    }
                }
                catch (TaskCanceledException) { /* Expected when mouse leaves or new hover starts */ }
            }, _hoverTokenSource.Token);
        }

        private async void ShowMapPreviewInternal(string? imageUrl, string mapName, string serverName, string currentMap, Point cursorPosition)
        {
            if (_mapPreviewWindow != null) return;
            // Defensive null check for _httpClient, though it should be initialized.
            if (_httpClient == null)
            {
                Debug.WriteLine("Error: _httpClient is null in ShowMapPreview.");
                return;
            }

            Form? newPreviewWindow = null; // Create locally first
            try
            {
                newPreviewWindow = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, ShowInTaskbar = false, BackColor = Color.Black, Size = new Size(384, 266), TopMost = true };
                newPreviewWindow.Click += (s, e) => newPreviewWindow?.Close();
                newPreviewWindow.Location = new Point(cursorPosition.X + 20, cursorPosition.Y + 20);

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
                        var image = await LoadPreviewImageAsync(imageUrl);
                        pictureBox.Image = image;
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

        private async Task<Image> LoadPreviewImageAsync(string imageUrl)
        {
            foreach (var candidateUrl in BuildImageUrlCandidates(imageUrl))
            {
                Debug.WriteLine($"ShowMapPreview: Attempting to download image from URL: {candidateUrl}");
                if (_imageCache.TryGetValue(candidateUrl, out var cachedImage))
                    return cachedImage;

                try
                {
                    using var cts = new CancellationTokenSource(5000);
                    using var stream = await _httpClient.GetStreamAsync(candidateUrl, cts.Token);
                    var image = Image.FromStream(stream);
                    if (_imageCache.Count < 50) _imageCache[candidateUrl] = image;
                    return image;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ShowMapPreview: Image download failed for {candidateUrl}: {ex.Message}");
                }
            }

            throw new InvalidOperationException($"No preview image candidate could be loaded for {imageUrl}");
        }

        private static IEnumerable<string> BuildImageUrlCandidates(string imageUrl)
        {
            yield return imageUrl;

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                yield break;

            string fileName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
            string extension = Path.GetExtension(uri.AbsolutePath);
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(extension))
                yield break;

            foreach (var candidateName in BuildFileNameCandidates(fileName).Skip(1))
            {
                var builder = new UriBuilder(uri)
                {
                    Path = uri.AbsolutePath[..^Path.GetFileName(uri.AbsolutePath).Length] + Uri.EscapeDataString(candidateName + extension)
                };
                yield return builder.Uri.ToString();
            }
        }

        private static IEnumerable<string> BuildFileNameCandidates(string fileName)
        {
            yield return fileName;

            string[] smallWords = { "a", "an", "and", "at", "by", "for", "from", "in", "of", "on", "or", "the", "to", "vs" };
            var parts = fileName.Split('-');
            if (parts.Length <= 1) yield break;

            var withSmallWords = parts
                .Select((part, index) => index == 0 || !smallWords.Contains(part, StringComparer.OrdinalIgnoreCase)
                    ? part
                    : part.ToLowerInvariant())
                .ToArray();

            string smallWordVariant = string.Join("-", withSmallWords);
            if (!smallWordVariant.Equals(fileName, StringComparison.Ordinal))
                yield return smallWordVariant;
        }

        public void CloseMapPreview()
        {
            _hoverTokenSource?.Cancel();
            _mapPreviewWindow?.Close();
            _mapPreviewWindow = null;
        }
    }
}
