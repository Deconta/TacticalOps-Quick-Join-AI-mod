namespace TacticalOpsQuickJoin
{
    internal static class Constants
    {
        // Network timeouts
        public const int DEFAULT_UDP_TIMEOUT = 1000;
        public const int MAP_DOWNLOAD_TIMEOUT = 15000;
        public const int MAP_PREVIEW_TIMEOUT = 5000;

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
        public const string MAP_SCREENSHOT_SMALL_BASE_URL = "https://mirror.tactical-ops.eu/map-screenshots/256x144-jpg/";
        public const string MAP_SCREENSHOT_PREVIEW_BASE_URL = "https://mirror.tactical-ops.eu/map-screenshots/640x360-jpg/";
        public const string MAP_SCREENSHOT_BIG_BASE_URL = "https://mirror.tactical-ops.eu/map-screenshots/1600x900-png/";

        // UI Icons
        public const string ICON_LOCKED = "\U0001F512";
    }
}
