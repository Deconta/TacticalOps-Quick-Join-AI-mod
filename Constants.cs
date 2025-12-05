namespace TacticalOpsQuickJoin
{
    internal static class Constants
    {
        // Network timeouts
        public const int DEFAULT_UDP_TIMEOUT = 1500;
        public const int MAP_DOWNLOAD_TIMEOUT = 15000;
        
        // Concurrency limits
        public const int MAX_CONCURRENT_PINGS = 100;
        
        // UI delays
        public const int MAP_PREVIEW_DELAY = 150;
        
        // Validation
        public const int MIN_PORT = 1024;
        public const int MAX_PORT = 65535;
        public const int MIN_REFRESH_INTERVAL = 30; // seconds
        public const int MAX_REFRESH_INTERVAL = 600; // seconds
        
        // URLs
        public const string MAP_JSON_URL = "https://raw.githubusercontent.com/InSource/TO-ServerStats/main/misc/maps/custom_maps.json";
        
        // UI Icons
        public const string ICON_LOCKED = "🔒";
    }
}
