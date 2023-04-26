using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using CliWrap;
using Microsoft.Data.SqlClient;

namespace Argentini.SqlPkg.Extensions;

public static class CliHelpers
{
	#region Constants
	
	public static List<string> ExportOptions => new()
	{
		// Commented items are replaced by SqlPkg

		"/AccessToken:",
		"/at:",
		// "/Action:",
		// "/a:",
		"/AzureCloudConfig:",
		"/acc:",
		"/Diagnostics:",
		"/d:",
		"/DiagnosticsFile:",
		"/df:",
		"/MaxParallelism:",
		"/mp:",
		"/OverwriteFiles:",
		"/of:",
		"/Properties:",
		"/p:",
		"/Quiet:",
		"/q:",
		// "/SourceConnectionString:",
		// "/scs:",
		// "/SourceDatabaseName:",
		// "/sdn:",
		"/SourceEncryptConnection:",
		"/sec:",
		"/SourceHostNameInCertificate:",
		"/shnic:",
		// "/SourcePassword:",
		// "/sp:",
		// "/SourceServerName:",
		// "/ssn:",
		// "/SourceTimeout:",
		// "/st:",
		// "/SourceTrustServerCertificate:",
		// "/stsc:",
		// "/SourceUser:",
		// "/su:",
		"/TargetFile:",
		"/tf:",
		"/TenantId:",
		"/tid:",
		"/ThreadMaxStackSize:",
		"/tmss:",
		"/UniversalAuthentication:",
		"/ua:",
		// "/p:CommandTimeout=",
		"/p:CompressionOption=",
		"/p:DatabaseLockTimeout=",
		"/p:HashObjectNamesInLogs=",
		"/p:IgnoreIndexesStatisticsOnEnclaveEnabledColumns=",
		"/p:LongRunningCommandTimeout=",
		"/p:Storage=",
		"/p:TableData=",
		"/p:TempDirectoryForTableData=",
		"/p:VerifyExtraction=",
		"/p:VerifyFullTextDocumentTypesSupported="
	};	

	public static List<string> ImportOptions => new()
	{
		// Commented items are replaced by SqlPkg
		
		"/AccessToken:",
		"/at:",
		// "/Action:",
		// "/a:",
		"/AzureCloudConfig:",
		"/acc:",
		"/Diagnostics:",
		"/d:",
		"/DiagnosticsFile:",
		"/df:",
		"/MaxParallelism:",
		"/mp:",
		"/ModelFilePath:",
		"/mfp:",
		"/Properties:",
		"/p:",
		"/Quiet:",
		"/q:",
		"/SourceFile:",
		"/sf:",
		// "/TargetConnectionString:",
		// "/tcs:",
		// "/TargetDatabaseName:",
		// "/tdn:",
		"/TargetEncryptConnection:",
		"/tec:",
		"/TargetHostNameInCertificate:",
		"/thnic:",
		// "/TargetPassword:",
		// "/tp:",
		// "/TargetServerName:",
		// "/tsn:",
		// "/TargetTimeout:",
		// "/tt:",
		// "/TargetTrustServerCertificate:",
		// "/ttsc:",
		// "/TargetUser:",
		// "/tu:",
		"/TenantId:",
		"/tid:",
		"/ThreadMaxStackSize:",
		"/tmss:",
		"/UniversalAuthentication:",
		"/ua:",
		// "/p:CommandTimeout=",
		"/p:DatabaseEdition=",
		"/p:DatabaseLockTimeout=",
		"/p:DatabaseMaximumSize=",
		"/p:DatabaseServiceObjective=",
		"/p:DisableIndexesForDataPhase=",
		"/p:DisableParallelismForEnablingIndexes=",
		"/p:HashObjectNamesInLogs=",
		"/p:ImportContributorArguments=",
		"/p:ImportContributorPaths=",
		"/p:ImportContributors=",
		"/p:LongRunningCommandTimeout=",
		"/p:PreserveIdentityLastValues=",
		"/p:RebuildIndexesOfflineForDataPhase=",
		"/p:Storage="
	};

	public static string RestoreExcludableObjects => @"ExcludeObjectTypes=Aggregates;ApplicationRoles;Assemblies;AssemblyFiles;AsymmetricKeys;BrokerPriorities;Certificates;ColumnEncryptionKeys;ColumnMasterKeys;Contracts;DatabaseOptions;DatabaseRoles;DatabaseTriggers;Defaults;ExtendedProperties;ExternalDataSources;ExternalFileFormats;ExternalTables;Filegroups;Files;FileTables;FullTextCatalogs;FullTextStoplists;MessageTypes;PartitionFunctions;PartitionSchemes;Permissions;Queues;RemoteServiceBindings;RoleMembership;Rules;ScalarValuedFunctions;SearchPropertyLists;SecurityPolicies;Sequences;Services;Signatures;StoredProcedures;SymmetricKeys;Synonyms;TableValuedFunctions;UserDefinedDataTypes;UserDefinedTableTypes;ClrUserDefinedTypes;Users;Views;XmlSchemaCollections;Audits;Credentials;CryptographicProviders;DatabaseAuditSpecifications;DatabaseEncryptionKeys;DatabaseScopedCredentials;Endpoints;ErrorMessages;EventNotifications;EventSessions;LinkedServerLogins;LinkedServers;Logins;MasterKeys;Routes;ServerAuditSpecifications;ServerRoleMembership;ServerRoles;ServerTriggers;ExternalStreams;ExternalStreamingJobs;DatabaseWorkloadGroups;WorkloadClassifiers;ExternalLibraries;ExternalLanguages";
	
	#endregion
	
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
	        return splits[1];

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
    
    #endregion
    
    #region Configuration

    /// <summary>
    /// Process server/database info to ensure connection strings exist.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="settings"></param>
    public static void NormalizeConnectionInfo(this string[] args, Settings settings)
    {
	    settings.SourceConnectionString = args.GetArgumentValue("/SourceConnectionString", "/scs", ':').Trim('\"');
	    settings.TargetConnectionString = args.GetArgumentValue("/TargetConnectionString", "/tcs", ':').Trim('\"');

	    #region Source
	    
	    if (string.IsNullOrEmpty(settings.SourceConnectionString) == false)
	    {
		    var builder = new SqlConnectionStringBuilder(settings.SourceConnectionString);

		    builder.TrustServerCertificate =
			    args.HasArgument("/SourceTrustServerCertificate:", "/stsc:")
				    ? args.GetArgumentValue("/SourceTrustServerCertificate", "/stsc", ':', "true").Equals("true", StringComparison.CurrentCultureIgnoreCase)
				    : builder.TrustServerCertificate;

		    builder.ConnectTimeout =
			    args.HasArgument("/SourceTimeout:", "/st:")
				    ? int.Parse(args.GetArgumentValue("/SourceTimeout", "/st", ':', "30"))
				    : builder.ConnectTimeout;
		    
		    builder.CommandTimeout =
			    args.HasArgument("/p:CommandTimeout=")
				    ? int.Parse(args.GetArgumentValue("/p:CommandTimeout", string.Empty, '=', "120"))
				    : builder.CommandTimeout;

		    settings.SourceServerName = builder.DataSource;
		    settings.SourceUserName = builder.UserID;
		    settings.SourcePassword = builder.Password;
		    settings.SourceDatabaseName = builder.InitialCatalog;
		    settings.SourceConnectionTimeout = builder.ConnectTimeout;
		    settings.SourceCommandTimeout = builder.CommandTimeout;
		    settings.SourceTrustServerCertificate = builder.TrustServerCertificate;
		    settings.SourceConnectionString = builder.ToString();
	    }

	    else
	    {
		    settings.SourceServerName = args.GetArgumentValue("/SourceServerName", "/ssn", ':');
		    settings.SourceDatabaseName = args.GetArgumentValue("/SourceDatabaseName", "/sdn", ':');
		    settings.SourceUserName = args.GetArgumentValue("/SourceUser", "/su", ':');
		    settings.SourcePassword = args.GetArgumentValue("/SourcePassword", "/sp", ':');
		    settings.SourceConnectionTimeout = int.Parse(args.GetArgumentValue("/SourceTimeout", "/st", ':', "30"));
		    settings.SourceCommandTimeout = int.Parse(args.GetArgumentValue("/p:CommandTimeout", string.Empty, '=', "120"));
		    settings.SourceTrustServerCertificate = args.GetArgumentValue("/SourceTrustServerCertificate", "/stsc", ':', "true").Equals("true", StringComparison.CurrentCultureIgnoreCase);

		    var builder = new SqlConnectionStringBuilder
		    {
			    DataSource = settings.SourceServerName,
			    InitialCatalog = settings.SourceDatabaseName,
			    UserID = settings.SourceUserName,
			    Password = settings.SourcePassword,
			    TrustServerCertificate = settings.SourceTrustServerCertificate,
			    ConnectTimeout = settings.SourceConnectionTimeout,
			    CommandTimeout = settings.SourceCommandTimeout
		    };

		    settings.SourceConnectionString = builder.ToString();
	    }

	    #endregion
	    
	    #region Target
	    
	    if (string.IsNullOrEmpty(settings.TargetConnectionString) == false)
	    {
		    var builder = new SqlConnectionStringBuilder(settings.TargetConnectionString);

		    builder.TrustServerCertificate =
			    args.HasArgument("/TargetTrustServerCertificate:", "/ttsc:")
				    ? args.GetArgumentValue("/TargetTrustServerCertificate", "/ttsc", ':', "true").Equals("true", StringComparison.CurrentCultureIgnoreCase)
				    : builder.TrustServerCertificate;

		    builder.ConnectTimeout =
			    args.HasArgument("/TargetTimeout:", "/tt:")
				    ? int.Parse(args.GetArgumentValue("/TargetTimeout", "/tt", ':', "30"))
				    : builder.ConnectTimeout;
		    
		    builder.CommandTimeout =
			    args.HasArgument("/p:CommandTimeout=")
				    ? int.Parse(args.GetArgumentValue("/p:CommandTimeout", string.Empty, '=', "120"))
				    : builder.CommandTimeout;

		    settings.TargetServerName = builder.DataSource;
		    settings.TargetUserName = builder.UserID;
		    settings.TargetPassword = builder.Password;
		    settings.TargetDatabaseName = builder.InitialCatalog;
		    settings.TargetConnectionTimeout = builder.ConnectTimeout;
		    settings.TargetCommandTimeout = builder.CommandTimeout;
		    settings.TargetTrustServerCertificate = builder.TrustServerCertificate;
		    settings.TargetConnectionString = builder.ToString();
	    }

	    else
	    {
		    settings.TargetServerName = args.GetArgumentValue("/TargetServerName", "/tsn", ':');
		    settings.TargetDatabaseName = args.GetArgumentValue("/TargetDatabaseName", "/tdn", ':');
		    settings.TargetUserName = args.GetArgumentValue("/TargetUser", "/tu", ':');
		    settings.TargetPassword = args.GetArgumentValue("/TargetPassword", "/tp", ':');
		    settings.TargetConnectionTimeout = int.Parse(args.GetArgumentValue("/TargetTimeout", "/tt", ':', "30"));
		    settings.TargetCommandTimeout = int.Parse(args.GetArgumentValue("/p:CommandTimeout", string.Empty, '=', "120"));
		    settings.TargetTrustServerCertificate = args.GetArgumentValue("/TargetTrustServerCertificate", "/ttsc", ':', "true").Equals("true", StringComparison.CurrentCultureIgnoreCase);

		    var builder = new SqlConnectionStringBuilder
		    {
			    DataSource = settings.TargetServerName,
			    InitialCatalog = settings.TargetDatabaseName,
			    UserID = settings.TargetUserName,
			    Password = settings.TargetPassword,
			    TrustServerCertificate = settings.TargetTrustServerCertificate,
			    ConnectTimeout = settings.TargetConnectionTimeout,
			    CommandTimeout = settings.TargetCommandTimeout
		    };

		    settings.TargetConnectionString = builder.ToString();
	    }

	    #endregion
    }

    /// <summary>
    /// Set defaults for Backup and Restore actions.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="allowed"></param>
    public static void BetterDefaults(this List<string> args, List<string> allowed)
    {
	    #region Better Defaults

	    if (allowed.Any(a => a.StartsWith("/p:IgnoreUserLoginMappings=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:IgnoreUserLoginMappings=", "true");
	
	    if (allowed.Any(a => a.StartsWith("/p:IgnorePermissions=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:IgnorePermissions=", "true");
	
	    if (allowed.Any(a => a.StartsWith("/p:VerifyExtraction=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:VerifyExtraction=", "false");

	    if (allowed.Any(a => a.StartsWith("/p:CreateNewDatabase=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:CreateNewDatabase=", "true");

	    #endregion
    }

    /// <summary>
    /// Create paths and remove existing files.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="argumentPrefix"></param>
    public static void EnsurePathAndDeleteExistingFile(this List<string> arguments, string argumentPrefix)
    {
	    var targetFileArg = arguments.FirstOrDefault(a => a.StartsWith(argumentPrefix));

	    if (string.IsNullOrEmpty(targetFileArg))
		    return;

	    var fileSplits = targetFileArg.Split(':', StringSplitOptions.RemoveEmptyEntries);

	    if (fileSplits.Length != 2)
		    return;

	    var fileName = fileSplits[1].Trim('\"');

	    #region Ensure Target Paths Exist

	    arguments.RemoveAll(a => a.Equals(targetFileArg, StringComparison.CurrentCultureIgnoreCase));
	    arguments.Add($"{fileSplits[0]}:\"{fileName}\"");
	    
	    if (fileName.Contains(Path.DirectorySeparatorChar) == false)
		    return;

	    var directoryPath = Path.GetDirectoryName(fileName) ?? string.Empty;
        
	    if (string.IsNullOrEmpty(directoryPath) == false && Directory.Exists(directoryPath) == false)
		    Directory.CreateDirectory(directoryPath);
	    
		File.Delete(fileName);
	    
	    #endregion
    }

    /*
    /// <summary>
    /// Process CLI arguments and filter based on allowed arguments for Action:Extract.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
	public static List<string> BuildExtractArguments(this IEnumerable<string> args)
	{
		var arguments = new List<string>();

		foreach (var arg in args)
		{
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');
		    
		    if (ExtractOptions.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)))
			    arguments.Add(arg);
		}

		arguments.Insert(0, "/a:Extract");
		arguments.Add("/p:ExtractAllTableData=false");
		arguments.SetArgumentFileExtension("/TargetFile:", ".dacpac", true);
		arguments.SetArgumentFileExtension("/DiagnosticsFile:", ".log", true, "schema");
		arguments.BetterDefaults(ExtractOptions);

		return arguments;
	}
	*/

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

		    if (ExportOptions.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)))
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Export");
	    arguments.Insert(1, $"/SourceConnectionString:\"{settings.SourceConnectionString}\"");
	    arguments.EnsurePathAndDeleteExistingFile("/TargetFile:");
	    arguments.EnsurePathAndDeleteExistingFile("/DiagnosticsFile:");
	    arguments.BetterDefaults(ExportOptions);
	    
	    return arguments;
    }

    /*
    /// <summary>
    /// Process CLI arguments and filter based on allowed arguments for Action:Extract.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static List<string> BuildPublishArguments(this IEnumerable<string> args)
    {
	    var arguments = new List<string>();

	    foreach (var arg in args)
	    {
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');
		    
		    if (PublishOptions.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)))
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Publish");
	    arguments.SetArgumentFileExtension("/SourceFile:", ".dacpac");
	    arguments.SetArgumentFileExtension("/DiagnosticsFile:", ".log", false, "schema");
	    arguments.BetterDefaults(PublishOptions);
	    
	    return arguments;
    }
	*/

    /// <summary>
    /// Process CLI arguments and filter based on allowed arguments for Action:Export.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static List<string> BuildImportArguments(this IEnumerable<string> args, Settings settings)
    {
	    var arguments = new List<string>();

	    foreach (var arg in args)
	    {
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');

		    if (ImportOptions.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)))
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Import");
	    arguments.Insert(1, $"/TargetConnectionString:\"{settings.TargetConnectionString}\"");
	    arguments.BetterDefaults(ImportOptions);
	    
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
                    tableDataList.Add(splits[1]);
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
                    var excludedTableName = exclusion.Split('=').Length == 2 ? exclusion.Split('=')[1] : string.Empty;

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
}