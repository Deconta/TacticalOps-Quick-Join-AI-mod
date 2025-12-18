#nullable enable

namespace TacticalOpsQuickJoin
{
    public interface ISettingsService
    {
        string? GetGamePath(string version);
        void SetGamePath(string version, string path);
        bool CloseOnJoin { get; set; }
        string MasterServers { get; set; }
        string FavoriteServers { get; set; }
        bool DarkMode { get; set; }
        int AutoRefreshInterval { get; set; }
        void Save();
    }
}
