using System.Diagnostics;
using System.Runtime.InteropServices;
using CliWrap;

namespace Argentini.SqlPkg.Extensions;

public static class CliHelpers
{
	#region Constants
	
	/// <summary>
	/// Bullet and space.
	/// </summary>
	public static string Bullet => "• ";

	public static string HeaderBar => "■";

	public static string HeaderBarMac => "▀";

	/// <summary>
	/// Ellipsis character.
	/// </summary>
	public static string Ellipsis => "…  ";
	
	/// <summary>
	/// Vertical bar character for console output (Windows)
	/// </summary>
	public static string Bar => "|";

	/// <summary>
	/// Vertical bar character for console output (Mac/Linux)
	/// </summary>
	public static string BarMac => "|";

	/// <summary>
	/// Arrow for console output (Windows)
	/// </summary>
	public static string Arrow => "→ ";

	/// <summary>
	/// Arrow for console output (Linux)
	/// </summary>
	public static string ArrowLinux => "➜  ";

	/// <summary>
	/// Arrow for console output (Mac)
	/// </summary>
	public static string ArrowMac => "→ ";

	/// <summary>
	/// Indentation for console output (Windows)
	/// </summary>
	public static string IndentationArrow => "  −→ ";

	/// <summary>
	/// Indentation for console output (Linux)
	/// </summary>
	public static string IndentationArrowLinux => "  ➜  ";

	/// <summary>
	/// Indentation for console output (Mac)
	/// </summary>
	public static string IndentationArrowMac => "  ⮑  ";
	
	#endregion
	
    #region Output Helpers
    
    /// <summary>
    /// Output an indentation arrow.
    /// </summary>
    public static void WriteIndentationArrow()
    {
	    Console.Write(GetIndentationArrow());
    }

    /// <summary>
    /// Output a separator arrow.
    /// </summary>
    /// <param name="leadingSpace"></param>
    public static void WriteArrow(bool leadingSpace = false)
    {
	    Console.Write((leadingSpace ? " " : string.Empty) + GetArrow());
    }

    /// <summary>
    /// Output a bullet.
    /// </summary>
    public static void WriteBullet()
    {
	    Console.Write(GetBullet());
    }

    /// <summary>
    /// Output a separator bar.
    /// </summary>
    public static void WriteBar()
    {
	    Console.Write(":");
    }
    
    /// <summary>
    /// Output the current date and time as Fri-10-Aug-2018.
    /// </summary>
    public static string GetDateTime()
    {
	    // Thu-2018-May-10 @ 12:30 PM
	    return DateTime.Now.ToString("ddd-dd-MMM-yyyy @ ") + DateTime.Now.ToString("h:mm:ss tt");
    }
	
    /// <summary>
    /// Get the best arrow for console output on the current platform.
    /// </summary>
    /// <returns>String with the arrow</returns>
    public static string GetArrow()
    {
	    var result = ArrowMac;

	    if (Identify.GetOsPlatform() == OSPlatform.Windows)
		    result = Arrow;

	    else if (Identify.GetOsPlatform() == OSPlatform.Linux)
		    result = ArrowLinux;

	    return result;
    }

    /// <summary>
    /// Get the best indentation arrow for console output on the current platform.
    /// </summary>
    /// <returns>String with the indented arrow</returns>
    public static string GetIndentationArrow()
    {
	    var result = IndentationArrowMac;

	    if (Identify.GetOsPlatform() == OSPlatform.Windows)
		    result = IndentationArrow;

	    else if (Identify.GetOsPlatform() == OSPlatform.Linux)
		    result = IndentationArrowLinux;

	    return result;
    }
    
    /// <summary>
    /// Get the bullet for console output on the current platform.
    /// </summary>
    /// <returns>String with the bullet</returns>
    public static string GetBullet()
    {
	    return Bullet;
    }
    
    /// <summary>
    /// Get the vertical bar for console output on the current platform.
    /// </summary>
    /// <returns>String with the vertical bar</returns>
    public static string GetBar()
    {
	    var result = BarMac;

	    if (Identify.GetOsPlatform() == OSPlatform.Windows)
		    result = Bar;

	    return result;
    }
    
    /// <summary>
    /// Get the thick title bar character for underliniing the app title on the current platform.
    /// </summary>
    /// <returns>String with the title bar character</returns>
    public static string GetHeaderBar()
    {
	    var result = HeaderBarMac;

	    if (Identify.GetOsPlatform() == OSPlatform.Windows)
		    result = HeaderBar;

	    return result;
    }
    
    /// <summary>
    /// Insert spaces to ensure a string is a specific width.
    /// Spaces are inserted in place of "{{gap}}". This only
    /// support one instance of the gap text.
    /// </summary>
    /// <param name="text">Text with {{gap}} in the middle for expansion</param>
    /// <param name="width">Column width</param>
    /// <returns>Text with {{gap}} expanded into spaces to make the text equal a given column width</returns>
    public static string FillWidth(this string text, int width)
    {
	    var result = text.Replace("{{gap}}", " ");

	    if (string.IsNullOrWhiteSpace(text))
		    return result;

	    if (width <= 0)
		    return result;
	    
	    var chunks = text.Split(new [] { "{{gap}}" }, StringSplitOptions.None);
	    var length = 0;

	    if (chunks.Length <= 1)
		    return result;
	    
	    foreach (var chunk in chunks)
	    {
		    length += chunk.Length;

		    result += chunk;
	    }

	    if (length >= width)
		    return result;
	    
	    result = string.Empty;

	    var gap = (width - length) / (chunks.Length - 1);

	    foreach (var chunk in chunks)
	    {
		    if (chunk == chunks.Last())
		    {
			    if ((width - length) % (chunks.Length - 1) > 0)
				    result += " ";
		    }

		    result += chunk;

		    if (chunk != chunks.Last())
		    {
			    result += new string(' ', gap);
		    }
	    }

	    return result;
    }

	#endregion

	#region Action Output

    /// <summary>
    /// Output list of excluded tables to the console.
    /// </summary>
    /// <param name="appState"></param>
    /// <returns></returns>
    public static string GetExcludedTableDataList(this ApplicationState appState)
    {
	    var result = string.Empty;
	    
	    foreach (var exclusion in appState.OriginalArguments.Where(a => a.Key.Equals("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
	    {
		    var excludedTableName = exclusion.Value.RemoveWrappedQuotes();

		    if (string.IsNullOrEmpty(excludedTableName))
			    continue;

		    if (string.IsNullOrEmpty(result) == false)
			    result += Environment.NewLine + " ".Repeat(10) + " ".Repeat(3);

		    result += excludedTableName.NormalizeTableName();
	    }

	    if (string.IsNullOrEmpty(result))
	    {
		    result += "All Tables";
	    }
	    else
	    {
		    result = "All Tables, Excluding:" + Environment.NewLine + " ".Repeat(10) + " ".Repeat(3) + result;
	    }
	    
	    return result;
    }
    
    /// <summary>
    /// Backup information output.
    /// </summary>
    /// <param name="appState"></param>
    public static void OutputBackupInfo(ApplicationState appState)
    {
	    Console.Write("Source    ");
	    WriteBar();
	    Console.Write("  " + appState.SourceServerName);
	    WriteArrow(true);
	    Console.WriteLine(appState.SourceDatabaseName);
	    
	    Console.Write("Target    ");
	    WriteBar();
	    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.TargetFile) == false ? appState.TargetFile : "None"));

	    Console.Write("Log File  ");
	    WriteBar();
	    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.LogFile) == false ? appState.LogFile : "None"));
	    
	    Console.Write("Data      ");
	    WriteBar();
	    Console.WriteLine("  " + appState.GetExcludedTableDataList());
    }

    /// <summary>
    /// Restore information output.
    /// </summary>
    /// <param name="appState"></param>
    public static void OutputRestoreInfo(ApplicationState appState)
    {
	    Console.Write("Source    ");
	    WriteBar();
	    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.SourceFile) == false ? appState.SourceFile : "None"));
                
	    Console.Write("Target    ");
	    WriteBar();
	    Console.Write("  " + appState.TargetServerName);
	    WriteArrow(true);
	    Console.WriteLine(appState.TargetDatabaseName);

	    Console.Write("Log File  ");
	    WriteBar();
	    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.LogFile) == false ? appState.LogFile : "None"));
    }

    /// <summary>
    /// Restore information output.
    /// </summary>
    /// <param name="appState"></param>
    public static void OutputCompleteInfo(ApplicationState appState)
    {
	    var backupActions = new[] { "Backup", "Backup-All", "Extract", "Export" };
	    
	    Console.Write("Source    ");
	    WriteBar();

	    if (backupActions.Contains(appState.Action, StringComparer.CurrentCultureIgnoreCase))
	    {
		    Console.Write("  " + appState.SourceServerName);
		    WriteArrow(true);

		    if (appState.Action.Equals("Backup-All", StringComparison.CurrentCultureIgnoreCase))
		    {
			    Console.WriteLine("All Databases");
		    }

		    else
		    {
			    Console.WriteLine(appState.SourceDatabaseName);
		    }		    
	    }

	    else
	    {
		    if (appState.Action.Equals("Restore-All", StringComparison.CurrentCultureIgnoreCase))
		    {
			    var path = appState.SourceFile;

			    path = path.ChangeFileNameInPath("*.bacpac");

			    Console.WriteLine($"  {path}");
		    }

		    else
		    {
			    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.SourceFile) == false ? appState.SourceFile : "None"));
		    }
	    }

	    Console.Write("Target    ");
	    WriteBar();
	    
	    if (backupActions.Contains(appState.Action, StringComparer.CurrentCultureIgnoreCase))
	    {
		    if (appState.Action.Equals("Backup-All", StringComparison.CurrentCultureIgnoreCase))
		    {
			    var path = appState.TargetFile;

			    path = path.ChangeFileNameInPath("*.bacpac");
			    
			    Console.WriteLine($"  {path}");
		    }

		    else
		    {
			    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.TargetFile) == false ? appState.TargetFile : "None"));
		    }
	    }

	    else
	    {
		    if (appState.Action.Equals("Restore-All", StringComparison.CurrentCultureIgnoreCase))
		    {
			    Console.WriteLine("  " + appState.TargetServerName);
		    }

		    else
		    {
			    Console.Write("  " + appState.TargetServerName);
			    WriteArrow(true);
			    Console.WriteLine(appState.TargetDatabaseName);
		    }
	    }

	    Console.Write("Log File  ");
	    WriteBar();

	    if (appState.Action.Equals("Backup-All", StringComparison.CurrentCultureIgnoreCase) || appState.Action.Equals("Restore-All", StringComparison.CurrentCultureIgnoreCase))
	    {
		    var path = string.IsNullOrEmpty(appState.LogFile) == false ? appState.LogFile : "None";

		    if (path != "None")
				path = path.ChangeFileNameInPath("*.log");
		    
		    Console.WriteLine($"  {path}");
	    }

	    else
	    {
		    Console.WriteLine("  " + (string.IsNullOrEmpty(appState.LogFile) == false ? appState.LogFile : "None"));
	    }
    }
    
    #endregion
    
    #region Execution

    public static async Task<int> ExecuteSqlPackageAsync(List<CliArgument> arguments, bool showOutput = true, bool showErrors = true)
    {
	    if (OperatingSystem.IsWindows() && arguments.Count < 2)
	    {
		    // BEGIN: Workaround for Windows Issues with CliWrap

		    var p = new Process();

		    p.StartInfo.UseShellExecute = false;
		    p.StartInfo.FileName = "sqlpackage.exe";
		    p.StartInfo.Arguments = arguments.GetArgumentsStringForCli();
		    p.StartInfo.RedirectStandardOutput = showOutput == false;
		    p.StartInfo.RedirectStandardError = showErrors == false;

		    if (showErrors == false)
			    p.ErrorDataReceived += (_, e) => { _ += e.Data; };
		    
		    p.Start();

		    if (showErrors == false)
			    p.BeginErrorReadLine();
		    
		    if (showOutput == false)
			    _ = await p.StandardOutput.ReadToEndAsync();  			    
		    
		    await p.WaitForExitAsync();

		    return p.ExitCode;

		    // END: Workaround for Windows Issues with CliWrap
	    }

	    await using var stdOut = Console.OpenStandardOutput();
	    
	    var cmd = Cli.Wrap("sqlpackage")
		    .WithArguments(arguments.GetArgumentsForCli())
		    .WithStandardOutputPipe(showOutput ? PipeTarget.ToStream(stdOut) : PipeTarget.Null)
		    .WithStandardErrorPipe(showErrors ? PipeTarget.ToStream(stdOut) : PipeTarget.Null);
		    
		var result = await cmd.ExecuteAsync();

	    return result.ExitCode;
    }
 
    #endregion
}