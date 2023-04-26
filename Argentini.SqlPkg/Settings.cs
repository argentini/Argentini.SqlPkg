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
    public string SourceFilePath { get; set; } = string.Empty;

    public string TargetConnectionString { get; set; } = string.Empty;
    public string TargetServerName { get; set; } = string.Empty;
    public string TargetDatabaseName { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;
    public string TargetPassword { get; set; } = string.Empty;
    public int TargetConnectionTimeout { get; set; } = 30;
    public int TargetCommandTimeout { get; set; } = 120;
    public bool TargetTrustServerCertificate { get; set; } = true;
    public string TargetFilePath { get; set; } = string.Empty;

    public string LogFilePath { get; set; } = string.Empty;
    
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