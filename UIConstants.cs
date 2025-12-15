namespace TacticalOpsQuickJoin;

public static class UIConstants
{
    // Magic numbers
    public const int MAX_PLAYERS = 64;
    public const int PING_TIMEOUT = 999;
    public const int IMAGE_DOWNLOAD_TIMEOUT = 5000;
    public const int FAVORITES_COLUMN_INDEX = 0;
    public const int MAP_COLUMN_INDEX = 2;
    public const int PLAYERS_COLUMN_INDEX = 3;
    public const int PING_COLUMN_INDEX = 4;
    public const int VERSION_COLUMN_INDEX = 5;
    
    // Colors - Dark Theme
    public static class DarkTheme
    {
        public static readonly Color Background = Color.FromArgb(32, 32, 32);
        public static readonly Color Foreground = Color.WhiteSmoke;
        public static readonly Color HeaderBackground = Color.FromArgb(70, 70, 70);
        public static readonly Color AccentColor = Color.FromArgb(100, 80, 30);
        public static readonly Color AlternatingRow = Color.FromArgb(40, 40, 40);
        public static readonly Color GridColor = Color.FromArgb(60, 60, 60);
        public static readonly Color MenuBorder = Color.FromArgb(70, 70, 70);
        public static readonly Color MenuSelected = Color.FromArgb(190, 190, 190);
        public static readonly Color MenuPressed = Color.FromArgb(160, 160, 160);
    }
    
    // Colors - Light Theme
    public static class LightTheme
    {
        public static readonly Color AlternatingRow = Color.FromArgb(248, 248, 248);
    }
    
    // Colors - Common
    public static class CommonColors
    {
        public static readonly Color JoinButtonBackground = Color.FromArgb(46, 204, 113);
        public static readonly Color JoinButtonForeground = Color.Black;
        
        // Ping colors
        public static readonly Color PingExcellent = Color.FromArgb(40, 90, 40);
        public static readonly Color PingGood = Color.FromArgb(100, 100, 40);
        public static readonly Color PingMedium = Color.FromArgb(120, 70, 30);
        public static readonly Color PingPoor = Color.FromArgb(100, 40, 40);
        public static readonly Color PingTimeout = Color.FromArgb(50, 50, 50);
        
        // Team colors
        public static readonly Color TeamRed = Color.FromArgb(180, 50, 50);
        public static readonly Color TeamBlue = Color.FromArgb(50, 50, 180);
        public static readonly Color TeamNone = Color.FromArgb(40, 40, 40);
        public static readonly Color TeamRedBright = Color.FromArgb(220, 80, 80);
        public static readonly Color TeamBlueBright = Color.FromArgb(80, 80, 220);
        public static readonly Color BotColor = Color.FromArgb(60, 60, 60);
        public static readonly Color BotForeground = Color.DarkGray;
    }
    
    // Ping thresholds
    public const int PING_EXCELLENT_THRESHOLD = 50;
    public const int PING_GOOD_THRESHOLD = 100;
    public const int PING_MEDIUM_THRESHOLD = 250;
    
    // Fonts
    public static class Fonts
    {
        public static Font HeaderFont => new("Segoe UI", 10, FontStyle.Bold);
        public static Font RegularFont => new("Segoe UI", 9, FontStyle.Regular);
        public static Font ButtonFont => new("Segoe UI", 11, FontStyle.Bold);
        public static Font MonospaceFont => new("Consolas", 10, FontStyle.Regular);
        public static Font StarFont => new("Segoe UI", 18, FontStyle.Regular);
    }
    
    // UI Dimensions
    public const int HEADER_HEIGHT = 40;
    public const int ROW_HEIGHT = 28;
    public const int JOIN_BUTTON_HEIGHT = 35;
    public const int FAVORITES_COLUMN_WIDTH = 25;
}
