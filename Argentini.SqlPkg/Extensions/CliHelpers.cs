using System.Runtime.InteropServices;
using System.Text;
using CliWrap;

namespace Argentini.SqlPkg.Extensions;

public static class CliHelpers
{
	#region Constants
	
	public static IEnumerable<string> ExportSkippedArguments => new []
	{
		"/Action:",
		"/a:",
		"/SourceConnectionString:",
		"/scs:",
		"/SourceDatabaseName:",
		"/sdn:",
		"/SourcePassword:",
		"/sp:",
		"/SourceServerName:",
		"/ssn:",
		"/SourceTimeout:",
		"/st:",
		"/SourceTrustServerCertificate:",
		"/stsc:",
		"/SourceUser:",
		"/su:",
		"/p:CommandTimeout=",
		"/p:ExcludeTableData="
	};	

	public static IEnumerable<string> ImportSkippedArguments => new []
	{
		"/Action:",
		"/a:",
		"/TargetConnectionString:",
		"/tcs:",
		"/TargetDatabaseName:",
		"/tdn:",
		"/TargetPassword:",
		"/tp:",
		"/TargetServerName:",
		"/tsn:",
		"/TargetTimeout:",
		"/tt:",
		"/TargetTrustServerCertificate:",
		"/ttsc:",
		"/TargetUser:",
		"/tu:",
		"/p:CommandTimeout="
	};

	public static string RestoreExcludableObjects => @"ExcludeObjectTypes=Aggregates;ApplicationRoles;Assemblies;AssemblyFiles;AsymmetricKeys;BrokerPriorities;Certificates;ColumnEncryptionKeys;ColumnMasterKeys;Contracts;DatabaseOptions;DatabaseRoles;DatabaseTriggers;Defaults;ExtendedProperties;ExternalDataSources;ExternalFileFormats;ExternalTables;Filegroups;Files;FileTables;FullTextCatalogs;FullTextStoplists;MessageTypes;PartitionFunctions;PartitionSchemes;Permissions;Queues;RemoteServiceBindings;RoleMembership;Rules;ScalarValuedFunctions;SearchPropertyLists;SecurityPolicies;Sequences;Services;Signatures;StoredProcedures;SymmetricKeys;Synonyms;TableValuedFunctions;UserDefinedDataTypes;UserDefinedTableTypes;ClrUserDefinedTypes;Users;Views;XmlSchemaCollections;Audits;Credentials;CryptographicProviders;DatabaseAuditSpecifications;DatabaseEncryptionKeys;DatabaseScopedCredentials;Endpoints;ErrorMessages;EventNotifications;EventSessions;LinkedServerLogins;LinkedServers;Logins;MasterKeys;Routes;ServerAuditSpecifications;ServerRoleMembership;ServerRoles;ServerTriggers;ExternalStreams;ExternalStreamingJobs;DatabaseWorkloadGroups;WorkloadClassifiers;ExternalLibraries;ExternalLanguages";

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
	public static string Arrow => " → ";

	/// <summary>
	/// Arrow for console output (Linux)
	/// </summary>
	public static string ArrowLinux => " ➜  ";

	/// <summary>
	/// Arrow for console output (Mac)
	/// </summary>
	public static string ArrowMac => " → ";

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
	
    #region Dependencies
    
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
    
    #endregion
    
    #region Argument Handling

    /// <summary>
    /// Determine if an argument has been passed on the command line.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="startsWith"></param>
    /// <param name="startsWithAbbrev"></param>
    /// <returns></returns>
    public static bool HasArgument(this IEnumerable<string>? arguments, string startsWith, string startsWithAbbrev = "")
    {
	    if (arguments == null)
		    return false;

	    var args = arguments.ToList();

	    if (args.Any() == false)
		    return false;

	    return args.Any(a =>
		    a.StartsWith(startsWith, StringComparison.CurrentCultureIgnoreCase) ||
	        (string.IsNullOrEmpty(startsWithAbbrev) == false && a.StartsWith(startsWithAbbrev, StringComparison.CurrentCultureIgnoreCase)));
    }
    
    /// <summary>
    /// Get a CLI argument value, or an emtpy string if not found.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="startsWith"></param>
    /// <param name="startsWithAbbrev"></param>
    /// <param name="delimiter"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string GetArgumentValue(this IEnumerable<string>? arguments, string startsWith, string startsWithAbbrev, char delimiter, string defaultValue = "")
    {
        if (arguments == null)
            return defaultValue;

        var args = arguments.ToList();

        if (args.Any() == false)
            return defaultValue;

        if (args.Any(a =>
	            a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) ||
	            (string.IsNullOrEmpty(startsWithAbbrev) == false && a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase))) == false)
	        return defaultValue;
        
        var splits = args.First(a => a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) || (string.IsNullOrEmpty(startsWithAbbrev) == false && a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase))).Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    
        if (splits.Length == 2)
	        return splits[1].Trim('\"').Trim('\'');

        return defaultValue;
    }

    /// <summary>
    /// Set a default CLI argument value if it doesn't exist.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="argumentPrefix"></param>
    /// <param name="appendValue"></param>
    /// <returns></returns>
    public static void SetDefault(this List<string> args, string argumentPrefix, string appendValue)
    {
        if (args.ToList().Any(a => a.StartsWith(argumentPrefix, StringComparison.CurrentCultureIgnoreCase)))
            return;
        
        args.Add($"{argumentPrefix}{appendValue}");
	}
    
    /// <summary>
    /// Create paths and remove existing files.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="argumentPrefix"></param>
    /// <param name="argumentAbbrevPrefix"></param>
    public static void EnsurePathAndDeleteExistingFile(this List<string> arguments, string argumentPrefix, string argumentAbbrevPrefix)
    {
	    var targetFileArg = arguments.FirstOrDefault(a => a.StartsWith(argumentPrefix));

	    if (string.IsNullOrEmpty(targetFileArg))
		    targetFileArg = arguments.FirstOrDefault(a => a.StartsWith(argumentAbbrevPrefix));
	    
	    if (string.IsNullOrEmpty(targetFileArg))
		    return;

	    var fileSplits = targetFileArg.Split(':', StringSplitOptions.RemoveEmptyEntries);

	    if (fileSplits.Length != 2)
		    return;

	    var fileName = fileSplits[1].Trim('\"').Trim('\"');

	    #region Ensure Target Paths Exist

	    arguments.RemoveAll(a => a.Equals(targetFileArg, StringComparison.CurrentCultureIgnoreCase));
	    arguments.Add($"{fileSplits[0].TrimEnd(':')}:\"{fileName}\"");
	    
	    if (fileName.Contains(Path.DirectorySeparatorChar) == false)
		    return;

	    var directoryPath = Path.GetDirectoryName(fileName) ?? string.Empty;
        
	    if (string.IsNullOrEmpty(directoryPath) == false && Directory.Exists(directoryPath) == false)
		    Directory.CreateDirectory(directoryPath);
	    
		File.Delete(fileName);
	    
	    #endregion
    }
    
    /// <summary>
    /// Process CLI arguments and filter based on allowed arguments for Action:Export.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static List<string> BuildExportArguments(this IEnumerable<string> args, Settings settings)
    {
	    var arguments = new List<string>();

	    foreach (var arg in args)
	    {
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');

		    if (ExportSkippedArguments.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)) == false)
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Export");
	    arguments.Insert(1, $"/SourceConnectionString:\"{settings.SourceConnectionString}\"");
	    arguments.EnsurePathAndDeleteExistingFile("/TargetFile:", "/tf:");
	    arguments.SetDefault("/p:VerifyExtraction=", "false");
	    
	    return arguments;
    }
    
    /// <summary>
    /// Process CLI arguments and filter based on allowed arguments for Action:Export.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static IEnumerable<string> BuildImportArguments(this IEnumerable<string> args, Settings settings)
    {
	    var arguments = new List<string>();

	    foreach (var arg in args)
	    {
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');

		    if (ImportSkippedArguments.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)) == false)
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Import");
	    arguments.Insert(1, $"/TargetConnectionString:\"{settings.TargetConnectionString}\"");
	    
	    return arguments;
    }
    
    /// <summary>
    /// Process table names and remove excluded tables from arguments.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="originalCliArgs"></param>
    /// <param name="settings"></param>
    public static async Task ProcessTableDataArguments(this List<string> arguments, IEnumerable<string> originalCliArgs, Settings settings)
    {
	    var tableDataList = new List<string>();
	    var args = originalCliArgs.ToList();	    

        if (args.Any(a => a.StartsWith("/p:TableData=", StringComparison.CurrentCultureIgnoreCase)))
        {
            foreach (var table in args.Where(a => a.StartsWith("/p:TableData=", StringComparison.CurrentCultureIgnoreCase)))
            {
                var splits = table.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (splits.Length == 2)
                {
                    tableDataList.Add(splits[1].Trim('\"').Trim('\''));
                }
            }
        }

        else
        {
            tableDataList.AddRange(await SqlTools.LoadUserTableNames(settings.SourceConnectionString));
            
            #region Handle Table Data Exclusions

            if (args.Any(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
            {
                foreach (var exclusion in args.Where(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
                {
                    var excludedTableName = exclusion.Split('=').Length == 2 ? exclusion.Split('=')[1].Trim('\"').Trim('\'') : string.Empty;

                    if (string.IsNullOrEmpty(excludedTableName))
                        continue;

                    excludedTableName = excludedTableName.NormalizeTableName();

                    if (excludedTableName.Contains('*'))
                    {
                        var wildcard = excludedTableName.Left("*");
                        tableDataList.RemoveAll(t => t.StartsWith(wildcard, StringComparison.CurrentCultureIgnoreCase));
                    }

                    else
                    {
                        tableDataList.RemoveAll(t => t.Equals(excludedTableName, StringComparison.CurrentCultureIgnoreCase));
                    }
                }

                var filteredArguments = new List<string>();
                
                foreach (var arg in arguments)
                {
                    if (arg.StartsWith("/p:TableData=", StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    filteredArguments.Add(arg);
                }

                arguments.Clear();
                arguments.AddRange(filteredArguments);
                arguments.AddRange(tableDataList.Select(tableName => $"/p:TableData={tableName}"));
            }

            #endregion
        }
    }
    
    #endregion
    
    #region Output Helpers

    public static string GetExcludedTableDataList(this IEnumerable<string> args)
    {
	    var result = string.Empty;
	    
	    foreach (var exclusion in args.Where(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
	    {
		    var excludedTableName = exclusion.Split('=').Length == 2 ? exclusion.Split('=')[1].Trim('\"').Trim('\'') : string.Empty;

		    if (string.IsNullOrEmpty(excludedTableName))
			    continue;

		    if (string.IsNullOrEmpty(result) == false)
				    result += Environment.NewLine + " ".Repeat(13) + " ".Repeat(3);

		    result += excludedTableName.NormalizeTableName();
	    }

	    if (string.IsNullOrEmpty(result))
	    {
		    result += "All Tables";
	    }
	    else
	    {
		    result = "All Tables, Excluding:" + Environment.NewLine + " ".Repeat(13) + " ".Repeat(3) + result;
	    }
	    
	    return result;
    }
    
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
    public static void WriteArrow()
    {
	    Console.Write(GetArrow());
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
	    Console.Write(GetBar());
    }
    
    /// <summary>
    /// Output the current date and time as Fri-10-Aug-2018.
    /// </summary>
    public static string GetDateTime()
    {
	    // Thu-2018-May-10 @ 12:30 PM
	    return DateTime.Now.ToString("ddd-dd-MMM-yyyy @ ") + DateTime.Now.ToString("h:mm tt");
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

    /// <summary>
    /// Backup information output.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="settings"></param>
    public static void OutputBackupInfo(IEnumerable<string> arguments, Settings settings)
    {
	    var args = arguments.ToList();
	    
	    Console.Write("Source       ");
	    WriteBar();
	    Console.Write("  " + settings.SourceServerName);
	    WriteArrow();
	    Console.WriteLine(settings.SourceDatabaseName);
	    Console.WriteLine();

	    Console.Write("Table Data   ");
	    WriteBar();
	    Console.WriteLine("  " + args.GetExcludedTableDataList());
	    Console.WriteLine();
                
	    Console.Write("Destination  ");
	    WriteBar();
	    Console.WriteLine("  " + (args.HasArgument("/TargetFile:", "/tf:") ? args.GetArgumentValue("/TargetFile", "/tf", ':').Trim('\"').Trim('\'') : "None"));
	    Console.WriteLine();
                
	    Console.Write("Log File     ");
	    WriteBar();
	    Console.WriteLine("  " + (args.HasArgument("/DiagnosticsFile:", "/df:") ? args.GetArgumentValue("/DiagnosticsFile", "/df", ':').Trim('\"').Trim('\'') : "None"));
	    Console.WriteLine();
    }

    /// <summary>
    /// Restore information output.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="settings"></param>
    public static void OutputRestoreInfo(IEnumerable<string> arguments, Settings settings)
    {
	    var args = arguments.ToList();
	    
	    Console.Write("Source       ");
	    WriteBar();
	    Console.WriteLine("  " + (args.HasArgument("/SourceFile:", "/sf:") ? args.GetArgumentValue("/SourceFile", "/sf", ':').Trim('\"').Trim('\'') : "None"));
	    Console.WriteLine();
                
	    Console.Write("Destination  ");
	    WriteBar();
	    Console.Write("  " + settings.TargetServerName);
	    WriteArrow();
	    Console.WriteLine(settings.TargetDatabaseName);
	    Console.WriteLine();

	    Console.Write("Log File     ");
	    WriteBar();
	    Console.WriteLine("  " + (args.HasArgument("/DiagnosticsFile:", "/df:") ? args.GetArgumentValue("/DiagnosticsFile", "/df", ':').Trim('\"').Trim('\'') : "None"));
	    Console.WriteLine();
    }
    
    #endregion
}