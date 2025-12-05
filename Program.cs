namespace TacticalOpsQuickJoin;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Global exception handlers for crash prevention
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        ApplicationConfiguration.Initialize();
        Application.Run(new FormMain());
    }
    
    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        LogError(e.Exception);
        MessageBox.Show($"An error occurred: {e.Exception.Message}\n\nThe application will continue running.", 
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogError(ex);
        }
    }
    
    private static void LogError(Exception ex)
    {
        try
        {
            string logPath = Path.Combine(Application.StartupPath, "error.log");
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";
            File.AppendAllText(logPath, logEntry);
        }
        catch { /* Ignore logging errors */ }
    }
}