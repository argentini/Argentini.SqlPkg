namespace Argentini.SqlPkg;

public class Settings
{
    public string Action { get; set; } = string.Empty;

    public string SourceConnectionString { get; set; } = string.Empty;
    public string SourceServerName { get; set; } = string.Empty;
    public string SourceDatabaseName { get; set; } = string.Empty;
    public string SourceUserName { get; set; } = string.Empty;
    public string SourcePassword { get; set; } = string.Empty;
    public int SourceConnectionTimeout { get; set; } = 30;
    public int SourceCommandTimeout { get; set; } = 120;
    public bool SourceTrustServerCertificate { get; set; } = true;

    public string TargetConnectionString { get; set; } = string.Empty;
    public string TargetServerName { get; set; } = string.Empty;
    public string TargetDatabaseName { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;
    public string TargetPassword { get; set; } = string.Empty;
    public int TargetConnectionTimeout { get; set; } = 30;
    public int TargetCommandTimeout { get; set; } = 120;
    public bool TargetTrustServerCertificate { get; set; } = true;
    
    /// <summary>
    /// Column width of the console output for items that require cropping.
    /// </summary>
    public static int ColumnWidth
    {
        get
        {
            const int minWidth = 76;
            const int maxWidth = 110;
            var currentWidth = Console.WindowWidth;

            if (currentWidth < minWidth)
                return minWidth;

            if (currentWidth > maxWidth)
                return maxWidth;

            return currentWidth;
        }
    }

    public static string AppMajorVersion
    {
        get
        {
            var result = string.Empty;

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                result = assembly.GetName().Version?.Major.ToString();
            }

            catch (Exception e)
            {
                Console.WriteLine($"Settings.AppMajorVersion Exception: {e.Message}");
            }

            return result ?? string.Empty;
        }
    }
    
    public static string AppMinorVersion
    {
        get
        {
            var result = string.Empty;

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                result = assembly.GetName().Version?.Minor.ToString();
            }

            catch (Exception e)
            {
                Console.WriteLine($"Settings.AppMinorVersion Exception: {e.Message}");
            }

            return result ?? string.Empty;
        }
    }
    
    public static string AppBuildVersion
    {
        get
        {
            var result = string.Empty;

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                result = assembly.GetName().Version?.Build.ToString();
            }

            catch (Exception e)
            {
                Console.WriteLine($"Settings.AppBuildVersion Exception: {e.Message}");
            }

            return result ?? string.Empty;
        }
    }

    public static string AppRevisionVersion
    {
        get
        {
            var result = string.Empty;

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                result = assembly.GetName().Version?.Revision.ToString();
            }

            catch (Exception e)
            {
                Console.WriteLine($"Settings.AppRevisionVersion Exception: {e.Message}");
            }

            return result ?? string.Empty;
        }
    }
    
    public static string Version
    {
        get
        {
            var result = string.Empty;

            try
            {
                result = AppMajorVersion + "." + AppMinorVersion + "." + AppBuildVersion;
            }

            catch (Exception e)
            {
                Console.WriteLine($"Settings.Version Exception: {e.Message}");
            }

            return result;
        }
    }
}