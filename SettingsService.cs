#nullable enable
using TacticalOpsQuickJoin.Properties;

namespace TacticalOpsQuickJoin
{
    public class SettingsService : ISettingsService
    {
        public string? GetGamePath(string version)
        {
            return Settings.Default[GetPropertyName(version)] as string;
        }

        public void SetGamePath(string version, string path)
        {
            Settings.Default[GetPropertyName(version)] = path;
        }

        public bool CloseOnJoin
        {
            get => Settings.Default.closeOnJoin;
            set => Settings.Default.closeOnJoin = value;
        }

        public string MasterServers
        {
            get => Settings.Default.masterservers;
            set => Settings.Default.masterservers = value;
        }

        public string FavoriteServers
        {
            get => Settings.Default.favoriteServers;
            set => Settings.Default.favoriteServers = value;
        }

        public bool DarkMode
        {
            get => Settings.Default.darkMode;
            set => Settings.Default.darkMode = value;
        }

        public int AutoRefreshInterval
        {
            get => Settings.Default.autoRefreshInterval;
            set => Settings.Default.autoRefreshInterval = value;
        }

        public void Save()
        {
            Settings.Default.Save();
        }

        private string GetPropertyName(string version)
        {
            return version switch
            {
                "2.2" => "to220path",
                "3.4" => "to340path",
                "3.5" => "to350path",
                _ => throw new ArgumentOutOfRangeException(nameof(version), $"Unknown game version: {version}")
            };
        }
    }
}
