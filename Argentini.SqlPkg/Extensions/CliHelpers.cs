using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using CliWrap;

namespace Argentini.SqlPkg.Extensions;

public static class CliHelpers
{
    #region OS and Runtime

    /// <summary>
    /// Get OS platform.
    /// </summary> 
    /// <returns>OSPlatform object</returns> 
    public static OSPlatform GetOsPlatform() 
    { 
        var osPlatform = OSPlatform.Create("Other platform");

        // Check if it's windows 
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows); 
        osPlatform = isWindows ? OSPlatform.Windows : osPlatform; 

        // Check if it's osx 
        var isOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX); 
        osPlatform = isOsx ? OSPlatform.OSX : osPlatform; 

        // Check if it's Linux 
        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux); 
        osPlatform = isLinux ? OSPlatform.Linux : osPlatform; 

        return osPlatform; 
    } 

    /// <summary>
    /// Get OS platform name for output to users.
    /// </summary> 
    /// <returns>OSPlatform name (friendly for output)</returns> 
    public static string GetOsPlatformName()
    {
        if (GetOsPlatform() == OSPlatform.OSX)
            return "macOS";

        if (GetOsPlatform() == OSPlatform.Linux)
            return "Linux";

        if (GetOsPlatform() == OSPlatform.Windows)
            return "Windows";

        return "Other";
    }

	/// <summary>
	/// Get the .NET Core runtime version (e.g. "2.2").
	/// </summary> 
	/// <returns>String with the .NET Core version number</returns> 
	public static string GetFrameworkVersion()
	{
		var result = Assembly
			.GetEntryAssembly()?
			.GetCustomAttribute<TargetFrameworkAttribute>()?
			.FrameworkName;

		if (result == null || result.IsEmpty()) return string.Empty;

		if (result.Contains("Version="))
			return result.Right("Version=").TrimStart(new [] { 'v' });

		return result;
	}

	/// <summary>
	/// Get the application version (e.g. "1.0.0.0").
	/// </summary> 
	/// <returns>String with the application version number</returns> 
	public static string GetAssemblyVersion()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

		return fileVersion.FileVersion ?? "0.0.0.0";
	}
	
	/// <summary>
	/// Get platform architecture (e.g. x64).
	/// </summary> 
	/// <returns>OSPlatform object</returns> 
	public static string GetPlatformArchitecture()
	{
		var architecture = RuntimeInformation.ProcessArchitecture.ToString();

		if (GetOsPlatformName() == "macOS" && architecture.Equals("Arm64", StringComparison.CurrentCultureIgnoreCase))
			architecture = "Apple Silicon";
		
		return architecture;
	}
	
	/// <summary>
	/// Get the .NET CLR runtime version (e.g. "4.6.27110.04").
	/// Only works in 2.2 or later.
	/// </summary> 
	/// <returns>String with the .NET CLR runtime version number</returns> 
	public static string GetRuntimeVersion()
	{
		return RuntimeInformation.FrameworkDescription.Right(" ");
	}

	/// <summary>
	/// Get the .NET CLR runtime version string.
	/// Only works in 2.2 or later.
	/// </summary> 
	/// <returns>String with the .NET CLR runtime version number</returns> 
	public static string GetRuntimeVersionFull()
	{
		return RuntimeInformation.FrameworkDescription;
	}

    #endregion
    
    /// <summary>
    /// Determine if SqlPackage is installed.
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> SqlPackageIsInstalled()
    {
        var sb = new StringBuilder();
        var cmd = Cli.Wrap("SqlPackage")
            .WithArguments(arguments => { arguments.Add("/version:true"); })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(sb));

        try
        {
            await cmd.ExecuteAsync();
            return true;
        }

        catch
        {
            Console.WriteLine("SqlPkg => Could not execute the 'SqlPackage' command.");
            Console.WriteLine("Be sure to install it using \"dotnet tool install -g microsoft.sqlpackage\".");
            Console.WriteLine("You will need the dotnet tool (version 6 or later) installed from \"https://dotnet.microsoft.com\" in order to install Microsoft SqlPackage.");
            
            return false;
        }
    }
    
    /// <summary>
    /// Get a CLI argument value, or an emtpy string if not found.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="startsWith"></param>
    /// <param name="startsWithAbbrev"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static string GetArgumentValue(this IEnumerable<string>? arguments, string startsWith, string startsWithAbbrev, char delimiter)
    {
        if (arguments == null)
            return string.Empty;

        var args = arguments.ToList();

        if (args.Any() == false)
            return string.Empty;
        
        if (args.Any(a => a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase)))
        {
            var splits = args.First(a => a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase)).Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                        
            if (splits.Length == 2)
            {
                return splits[1];
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Set a default CLI argument value if it doesn't exist.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="argumentPrefix"></param>
    /// <param name="appendValue"></param>
    /// <returns></returns>
    public static string[] SetDefault(this string[] args, string argumentPrefix, string appendValue)
    {
        if (args.Any(a => a.StartsWith(argumentPrefix, StringComparison.CurrentCultureIgnoreCase)))
            return args;
        
        var tempArgs = args.ToList();
            
        tempArgs.Add($"{argumentPrefix}{appendValue}");
            
        return tempArgs.ToArray();
    }
}