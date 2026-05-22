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
        public static readonly Color Background = Color.FromArgb(18, 20, 19);
        public static readonly Color Surface = Color.FromArgb(26, 29, 27);
        public static readonly Color SurfaceRaised = Color.FromArgb(34, 38, 35);
        public static readonly Color Foreground = Color.FromArgb(236, 240, 234);
        public static readonly Color MutedForeground = Color.FromArgb(166, 174, 163);
        public static readonly Color HeaderBackground = Color.FromArgb(45, 54, 47);
        public static readonly Color AccentColor = Color.FromArgb(50, 135, 91);
        public static readonly Color AccentHover = Color.FromArgb(65, 157, 109);
        public static readonly Color AlternatingRow = Color.FromArgb(22, 25, 23);
        public static readonly Color GridColor = Color.FromArgb(45, 51, 47);
        public static readonly Color MenuBorder = Color.FromArgb(55, 64, 57);
        public static readonly Color MenuSelected = Color.FromArgb(46, 63, 53);
        public static readonly Color MenuPressed = Color.FromArgb(55, 88, 68);
    }
    
    // Colors - Light Theme
    public static class LightTheme
    {
        public static readonly Color Background = Color.FromArgb(242, 244, 240);
        public static readonly Color Surface = Color.FromArgb(252, 253, 250);
        public static readonly Color SurfaceRaised = Color.FromArgb(233, 237, 229);
        public static readonly Color Foreground = Color.FromArgb(28, 34, 30);
        public static readonly Color MutedForeground = Color.FromArgb(94, 105, 94);
        public static readonly Color HeaderBackground = Color.FromArgb(216, 226, 211);
        public static readonly Color AccentColor = Color.FromArgb(42, 125, 83);
        public static readonly Color AccentHover = Color.FromArgb(36, 145, 91);
        public static readonly Color AlternatingRow = Color.FromArgb(247, 249, 245);
        public static readonly Color GridColor = Color.FromArgb(216, 224, 212);
        public static readonly Color MenuSelected = Color.FromArgb(218, 229, 216);
        public static readonly Color MenuPressed = Color.FromArgb(204, 222, 207);
    }
    
    // Colors - Common
    public static class CommonColors
    {
        public static readonly Color JoinButtonBackground = Color.FromArgb(44, 151, 95);
        public static readonly Color JoinButtonForeground = Color.White;
        
        // Ping colors
        public static readonly Color PingExcellent = Color.FromArgb(42, 118, 79);
        public static readonly Color PingGood = Color.FromArgb(118, 120, 52);
        public static readonly Color PingMedium = Color.FromArgb(147, 91, 43);
        public static readonly Color PingPoor = Color.FromArgb(137, 55, 53);
        public static readonly Color PingTimeout = Color.FromArgb(66, 70, 67);
        
        // Team colors
        public static readonly Color TeamRed = Color.FromArgb(132, 46, 48);
        public static readonly Color TeamBlue = Color.FromArgb(55, 61, 145);
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
        public static Font HeaderFont => new("Segoe UI Semibold", 9.25f, FontStyle.Regular);
        public static Font MenuFont => new("Segoe UI", 9.0f, FontStyle.Regular);
        public static Font RegularFont => new("Segoe UI", 8.75f, FontStyle.Regular);
        public static Font ButtonFont => new("Segoe UI Semibold", 9.5f, FontStyle.Regular);
        public static Font MonospaceFont => new("Consolas", 8.75f, FontStyle.Regular);
        public static Font StarFont => new("Segoe UI", 12f, FontStyle.Regular);
    }
    
    // UI Dimensions
    public const int HEADER_HEIGHT = 34;
    public const int ROW_HEIGHT = 25;
    public const int JOIN_BUTTON_HEIGHT = 34;
    public const int FAVORITES_COLUMN_WIDTH = 36;

    public const string FavoriteStar = "\u2605";
    public const string FavoriteEmptyStar = "\u2606";
}
