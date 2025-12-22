#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public class ServerListManager
    {
        private readonly ISettingsService _settingsService;
        private readonly IServerProvider _serverProvider;

        public List<ServerData> Servers { get; } = [];
        public Dictionary<int, ServerData> ServerLookup { get; } = new();

        public HashSet<string> FavoriteServers { get; } = [];
        public HashSet<string> IgnoredServers { get; } = [];

        public ServerListManager(ISettingsService settingsService, IServerProvider serverProvider)
        {
            _settingsService = settingsService;
            _serverProvider = serverProvider;
            LoadFavorites();
            LoadIgnoredServers();
        }

        public async Task RefreshServerListAsync(Action<ServerData> onServerAdded)
        {
            Servers.Clear();
            ServerLookup.Clear();

            await _serverProvider.GetServerListAsync(server =>
            {
                AddServer(server);
                onServerAdded(server);
            });
        }

        private void AddServer(ServerData serverData)
        {
            Servers.Add(serverData);
            ServerLookup[serverData.Id] = serverData;
        }

        private void LoadFavorites()
        {
            try
            {
                var favorites = (_settingsService.FavoriteServers ?? "").Split(',');
                foreach (var fav in favorites.Where(f => !string.IsNullOrWhiteSpace(f)))
                    FavoriteServers.Add(fav.Trim());
            }
            catch { }
        }

        private void SaveFavorites()
        {
            try
            {
                _settingsService.FavoriteServers = string.Join(",", FavoriteServers);
                _settingsService.Save();
            }
            catch { }
        }

        public void ToggleFavorite(string serverKey)
        {
            if (!FavoriteServers.Remove(serverKey))
                FavoriteServers.Add(serverKey);
            SaveFavorites();
        }

        private void LoadIgnoredServers()
        {
            try
            {
                var ignored = (_settingsService.IgnoredServers ?? "").Split(',');
                foreach (var ignoredServer in ignored.Where(f => !string.IsNullOrWhiteSpace(f)))
                    IgnoredServers.Add(ignoredServer.Trim());
            }
            catch { }
        }

        private void SaveIgnoredServers()
        {
            try
            {
                _settingsService.IgnoredServers = string.Join(",", IgnoredServers);
                _settingsService.Save();
            }
            catch { }
        }

        public void ToggleIgnoreServer(string serverKey)
        {
            if (!IgnoredServers.Remove(serverKey))
                IgnoredServers.Add(serverKey);
            SaveIgnoredServers();
        }
    }
}