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
	
	// public static List<string> ExtractOptions => new()
	// {
	// 	"/AccessToken:",
	// 	"/at:",
	// 	//"/Action:",
	// 	//"/a:",
	// 	"/AzureCloudConfig:",
	// 	"/acc:",
	// 	"/Diagnostics:",
	// 	"/d:",
	// 	"/DiagnosticsFile:",
	// 	"/df:",
	// 	"/MaxParallelism:",
	// 	"/mp:",
	// 	"/OverwriteFiles:",
	// 	"/of:",
	// 	"/Properties:",
	// 	"/p:",
	// 	"/Quiet:",
	// 	"/q:",
	// 	"/SourceConnectionString:",
	// 	"/scs:",
	// 	"/SourceDatabaseName:",
	// 	"/sdn:",
	// 	"/SourceEncryptConnection:",
	// 	"/sec:",
	// 	"/SourceHostNameInCertificate:",
	// 	"/shnic:",
	// 	"/SourcePassword:",
	// 	"/sp:",
	// 	"/SourceServerName:",
	// 	"/ssn:",
	// 	"/SourceTimeout:",
	// 	"/st:",
	// 	"/SourceTrustServerCertificate:",
	// 	"/stsc:",
	// 	"/SourceUser:",
	// 	"/su:",
	// 	"/TargetFile:",
	// 	"/tf:",
	// 	"/TenantId:",
	// 	"/tid:",
	// 	"/ThreadMaxStackSize:",
	// 	"/tmss:",
	// 	"/UniversalAuthentication:",
	// 	"/ua:",
	// 	"/p:AzureStorageBlobEndpoint=",
	// 	"/p:AzureStorageContainer=",
	// 	"/p:AzureStorageKey=",
	// 	"/p:AzureStorageRootPath=",
	// 	"/p:CommandTimeout=",
	// 	"/p:CompressionOption=",
	// 	"/p:DacApplicationDescription=",
	// 	"/p:DacApplicationName=",
	// 	"/p:DacMajorVersion=",
	// 	"/p:DacMinorVersion=",
	// 	"/p:DatabaseLockTimeout=",
	// 	//"/p:ExtractAllTableData=", // Disallow for schema arguments
	// 	"/p:ExtractApplicationScopedObjectsOnly=",
	// 	"/p:ExtractReferencedServerScopedElements=",
	// 	"/p:ExtractTarget=",
	// 	"/p:ExtractUsageProperties=",
	// 	"/p:HashObjectNamesInLogs=",
	// 	"/p:IgnoreExtendedProperties=",
	// 	"/p:IgnorePermissions=",
	// 	"/p:IgnoreUserLoginMappings=",
	// 	"/p:LongRunningCommandTimeout=",
	// 	"/p:Storage=",
	// 	//"/p:TableData=", // Disallow for schema arguments
	// 	//"/p:TempDirectoryForTableData=", // Disallow for schema arguments
	// 	"/p:VerifyExtraction="
	// };	

	public static List<string> ExportOptions => new()
	{
		"/AccessToken:",
		"/at:",
		//"/Action:",
		//"/a:",
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
		"/SourceConnectionString:",
		"/scs:",
		"/SourceDatabaseName:",
		"/sdn:",
		"/SourceEncryptConnection:",
		"/sec:",
		"/SourceHostNameInCertificate:",
		"/shnic:",
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
		"/TargetFile:",
		"/tf:",
		"/TenantId:",
		"/tid:",
		"/ThreadMaxStackSize:",
		"/tmss:",
		"/UniversalAuthentication:",
		"/ua:",
		"/p:CommandTimeout=",
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

	// public static List<string> PublishOptions => new()
	// {
	// 	"/AccessToken:",
	// 	"/at:",
	// 	//"/Action:",
	// 	//"/a:",
	// 	"/AzureCloudConfig:",
	// 	"/acc:",
	// 	"/AzureKeyVaultAuthMethod:",
	// 	"/akv:",
	// 	"/ClientId:",
	// 	"/cid:",
	// 	"/DeployReportPath:",
	// 	"/drp:",
	// 	"/DeployScriptPath:",
	// 	"/dsp:",
	// 	"/Diagnostics:",
	// 	"/d:",
	// 	"/DiagnosticsFile:",
	// 	"/df:",
	// 	"/MaxParallelism:",
	// 	"/mp:",
	// 	"/ModelFilePath:",
	// 	"/mfp:",
	// 	"/OverwriteFiles:",
	// 	"/of:",
	// 	"/Profile:",
	// 	"/pr:",
	// 	"/Properties:",
	// 	"/p:",
	// 	"/Quiet:",
	// 	"/q:",
	// 	"/ReferencePaths:",
	// 	"/rp:",
	// 	"/Secret:",
	// 	"/secr:",
	// 	"/SourceFile:",
	// 	"/sf:",
	// 	"/SourceConnectionString:",
	// 	"/scs:",
	// 	"/SourceDatabaseName:",
	// 	"/sdn:",
	// 	"/SourceEncryptConnection:",
	// 	"/sec:",
	// 	"/SourceHostNameInCertificate:",
	// 	"/shnic:",
	// 	"/SourcePassword:",
	// 	"/sp:",
	// 	"/SourceServerName:",
	// 	"/ssn:",
	// 	"/SourceTimeout:",
	// 	"/st:",
	// 	"/SourceTrustServerCertificate:",
	// 	"/stsc:",
	// 	"/SourceUser:",
	// 	"/su:",
	// 	"/TargetConnectionString:",
	// 	"/tcs:",
	// 	"/TargetDatabaseName:",
	// 	"/tdn:",
	// 	"/TargetEncryptConnection:",
	// 	"/tec:",
	// 	"/TargetHostNameInCertificate:",
	// 	"/thnic:",
	// 	"/TargetPassword:",
	// 	"/tp:",
	// 	"/TargetServerName:",
	// 	"/tsn:",
	// 	"/TargetTimeout:",
	// 	"/tt:",
	// 	"/TargetTrustServerCertificate:",
	// 	"/ttsc:",
	// 	"/TargetUser:",
	// 	"/tu:",
	// 	"/TenantId:",
	// 	"/tid:",
	// 	"/ThreadMaxStackSize:",
	// 	"/tmss:",
	// 	"/UniversalAuthentication:",
	// 	"/ua:",
	// 	"/Variables:",
	// 	"/v:",
	// 	"/p:AdditionalDeploymentContributorArguments=",
	// 	"/p:AdditionalDeploymentContributorPaths=",
	// 	"/p:AdditionalDeploymentContributors=",
	// 	"/p:AllowDropBlockingAssemblies=",
	// 	"/p:AllowExternalLanguagePaths=",
	// 	"/p:AllowExternalLibraryPaths=",
	// 	"/p:AllowIncompatiblePlatform=",
	// 	"/p:AllowUnsafeRowLevelSecurityDataMovement=",
	// 	"/p:AzureSharedAccessSignatureToken=",
	// 	"/p:AzureStorageBlobEndpoint=",
	// 	"/p:AzureStorageContainer=",
	// 	"/p:AzureStorageKey=",
	// 	"/p:AzureStorageRootPath=",
	// 	"/p:BackupDatabaseBeforeChanges=",
	// 	"/p:BlockOnPossibleDataLoss=",
	// 	"/p:BlockWhenDriftDetected=",
	// 	"/p:CommandTimeout=",
	// 	"/p:CommentOutSetVarDeclarations=",
	// 	"/p:CompareUsingTargetCollation=",
	// 	"/p:CreateNewDatabase=",
	// 	"/p:DatabaseEdition=",
	// 	"/p:DatabaseLockTimeout=",
	// 	"/p:DatabaseMaximumSize=",
	// 	"/p:DatabaseServiceObjective=",
	// 	"/p:DeployDatabaseInSingleUserMode=",
	// 	"/p:DisableAndReenableDdlTriggers=",
	// 	"/p:DisableIndexesForDataPhase=",
	// 	"/p:DisableParallelismForEnablingIndexes=",
	// 	"/p:DoNotAlterChangeDataCaptureObjects=",
	// 	"/p:DoNotAlterReplicatedObjects=",
	// 	"/p:DoNotDropDatabaseWorkloadGroups=",
	// 	"/p:DoNotDropObjectType=",
	// 	"/p:DoNotDropObjectTypes=",
	// 	"/p:DoNotDropWorkloadClassifiers=",
	// 	"/p:DoNotEvaluateSqlCmdVariables=",
	// 	"/p:DropConstraintsNotInSource=",
	// 	"/p:DropDmlTriggersNotInSource=",
	// 	"/p:DropExtendedPropertiesNotInSource=",
	// 	"/p:DropIndexesNotInSource=",
	// 	"/p:DropObjectsNotInSource=",
	// 	"/p:DropPermissionsNotInSource=",
	// 	"/p:DropRoleMembersNotInSource=",
	// 	"/p:DropStatisticsNotInSource=",
	// 	"/p:EnclaveAttestationProtocol=",
	// 	"/p:EnclaveAttestationUrl=",
	// 	"/p:ExcludeObjectType=",
	// 	"/p:ExcludeObjectTypes=",
	// 	"/p:GenerateSmartDefaults=",
	// 	"/p:HashObjectNamesInLogs=",
	// 	"/p:IgnoreAnsiNulls=",
	// 	"/p:IgnoreAuthorizer=",
	// 	"/p:IgnoreColumnCollation=",
	// 	"/p:IgnoreColumnOrder=",
	// 	"/p:IgnoreComments=",
	// 	"/p:IgnoreCryptographicProviderFilePath=",
	// 	"/p:IgnoreDatabaseWorkloadGroups=",
	// 	"/p:IgnoreDdlTriggerOrder=",
	// 	"/p:IgnoreDdlTriggerState=",
	// 	"/p:IgnoreDefaultSchema=",
	// 	"/p:IgnoreDmlTriggerOrder=",
	// 	"/p:IgnoreDmlTriggerState=",
	// 	"/p:IgnoreExtendedProperties=",
	// 	"/p:IgnoreFileAndLogFilePath=",
	// 	"/p:IgnoreFilegroupPlacement=",
	// 	"/p:IgnoreFileSize=",
	// 	"/p:IgnoreFillFactor=",
	// 	"/p:IgnoreFullTextCatalogFilePath=",
	// 	"/p:IgnoreIdentitySeed=",
	// 	"/p:IgnoreIncrement=",
	// 	"/p:IgnoreIndexOptions=",
	// 	"/p:IgnoreIndexPadding=",
	// 	"/p:IgnoreKeywordCasing=",
	// 	"/p:IgnoreLockHintsOnIndexes=",
	// 	"/p:IgnoreLoginSids=",
	// 	"/p:IgnoreNotForReplication=",
	// 	"/p:IgnoreObjectPlacementOnPartitionScheme=",
	// 	"/p:IgnorePartitionSchemes=",
	// 	"/p:IgnorePermissions=",
	// 	"/p:IgnoreQuotedIdentifiers=",
	// 	"/p:IgnoreRoleMembership=",
	// 	"/p:IgnoreRouteLifetime=",
	// 	"/p:IgnoreSemicolonBetweenStatements=",
	// 	"/p:IgnoreSensitivityClassifications=",
	// 	"/p:IgnoreTableOptions=",
	// 	"/p:IgnoreTablePartitionOptions=",
	// 	"/p:IgnoreUserSettingsObjects=",
	// 	"/p:IgnoreWhitespace=",
	// 	"/p:IgnoreWithNocheckOnCheckConstraints=",
	// 	"/p:IgnoreWithNocheckOnForeignKeys=",
	// 	"/p:IgnoreWorkloadClassifiers=",
	// 	"/p:IncludeCompositeObjects=",
	// 	"/p:IncludeTransactionalScripts=",
	// 	"/p:IsAlwaysEncryptedParameterizationEnabled=",
	// 	"/p:LongRunningCommandTimeout=",
	// 	"/p:NoAlterStatementsToChangeClrTypes=",
	// 	"/p:PopulateFilesOnFileGroups=",
	// 	"/p:PreserveIdentityLastValues=",
	// 	"/p:RebuildIndexesOfflineForDataPhase=",
	// 	"/p:RegisterDataTierApplication=",
	// 	"/p:RestoreSequenceCurrentValue=",
	// 	"/p:RunDeploymentPlanExecutors=",
	// 	"/p:ScriptDatabaseCollation=",
	// 	"/p:ScriptDatabaseCompatibility=",
	// 	"/p:ScriptDatabaseOptions=",
	// 	"/p:ScriptDeployStateChecks=",
	// 	"/p:ScriptFileSize=",
	// 	"/p:ScriptNewConstraintValidation=",
	// 	"/p:ScriptRefreshModule=",
	// 	"/p:Storage=",
	// 	"/p:TreatVerificationErrorsAsWarnings=",
	// 	"/p:UnmodifiableObjectWarnings=",
	// 	"/p:VerifyCollationCompatibility=",
	// 	"/p:VerifyDeployment="
	// };	
	
	public static List<string> ImportOptions => new()
	{
		"/AccessToken:",
		"/at:",
		//"/Action:",
		//"/a:",
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
		"/TargetConnectionString:",
		"/tcs:",
		"/TargetDatabaseName:",
		"/tdn:",
		"/TargetEncryptConnection:",
		"/tec:",
		"/TargetHostNameInCertificate:",
		"/thnic:",
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
		"/TenantId:",
		"/tid:",
		"/ThreadMaxStackSize:",
		"/tmss:",
		"/UniversalAuthentication:",
		"/ua:",
		"/p:CommandTimeout=",
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

        if (!args.Any(a =>
	            a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) ||
	            a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase)))
	        return string.Empty;
        
        var splits = args.First(a => a.StartsWith($"{startsWith}{delimiter}", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith($"{startsWithAbbrev}{delimiter}", StringComparison.CurrentCultureIgnoreCase)).Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    
        if (splits.Length == 2)
	        return splits[1];

        return string.Empty;
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
	    #region Source
            
	    settings.SourceConnectionString = args.GetArgumentValue("/SourceConnectionString", "/scs", ':');
	    settings.SourceServerName = args.GetArgumentValue("/SourceServerName", "/ssn", ':');
	    settings.SourceDatabaseName = args.GetArgumentValue("/SourceDatabaseName", "/sdn", ':');
	    settings.SourceUserName = args.GetArgumentValue("/SourceUser", "/su", ':');
	    settings.SourcePassword = args.GetArgumentValue("/SourcePassword", "/sp", ':');

	    if (string.IsNullOrEmpty(settings.SourceConnectionString) == false)
	    {
		    var builder = new SqlConnectionStringBuilder(settings.SourceConnectionString)
		    {
			    TrustServerCertificate = true,
			    ConnectTimeout = 30,
			    CommandTimeout = 120
		    };

		    settings.SourceConnectionString = builder.ToString();

		    settings.SourceServerName = builder.DataSource;
		    settings.SourceUserName = builder.UserID;
		    settings.SourcePassword = builder.Password;
		    settings.SourceDatabaseName = builder.InitialCatalog;
	    }

	    else if (string.IsNullOrEmpty(settings.SourceServerName) == false)
	    {
		    var builder = new SqlConnectionStringBuilder
		    {
			    DataSource = settings.SourceServerName,
			    InitialCatalog = settings.SourceDatabaseName,
			    UserID = settings.SourceUserName,
			    Password = settings.SourcePassword,
			    TrustServerCertificate = true,
			    Authentication = SqlAuthenticationMethod.SqlPassword,
			    ConnectTimeout = 30,
			    CommandTimeout = 120
		    };

		    settings.SourceConnectionString = builder.ToString();
	    }

	    #endregion
	    
	    #region Target
            
	    settings.TargetConnectionString = args.GetArgumentValue("/TargetConnectionString", "/scs", ':');
	    settings.TargetServerName = args.GetArgumentValue("/TargetServerName", "/ssn", ':');
	    settings.TargetDatabaseName = args.GetArgumentValue("/TargetDatabaseName", "/sdn", ':');
	    settings.TargetUserName = args.GetArgumentValue("/TargetUser", "/su", ':');
	    settings.TargetPassword = args.GetArgumentValue("/TargetPassword", "/sp", ':');

	    if (string.IsNullOrEmpty(settings.TargetConnectionString) == false)
	    {
		    var builder = new SqlConnectionStringBuilder(settings.TargetConnectionString)
		    {
			    TrustServerCertificate = true,
			    ConnectTimeout = 30,
			    CommandTimeout = 120
		    };

		    settings.TargetConnectionString = builder.ToString();

		    settings.TargetServerName = builder.DataSource;
		    settings.TargetUserName = builder.UserID;
		    settings.TargetPassword = builder.Password;
		    settings.TargetDatabaseName = builder.InitialCatalog;
	    }

	    else if (string.IsNullOrEmpty(settings.TargetServerName) == false)
	    {
		    var builder = new SqlConnectionStringBuilder
		    {
			    DataSource = settings.TargetServerName,
			    InitialCatalog = settings.TargetDatabaseName,
			    UserID = settings.TargetUserName,
			    Password = settings.TargetPassword,
			    TrustServerCertificate = true,
			    Authentication = SqlAuthenticationMethod.SqlPassword,
			    ConnectTimeout = 30,
			    CommandTimeout = 120
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

	    if (allowed.Any(a => a.StartsWith("/SourceTrustServerCertificate:", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/SourceTrustServerCertificate:", "true");
	    
	    if (allowed.Any(a => a.StartsWith("/p:IgnoreUserLoginMappings=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:IgnoreUserLoginMappings=", "true");
	
	    if (allowed.Any(a => a.StartsWith("/p:IgnorePermissions=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:IgnorePermissions=", "true");
	
	    if (allowed.Any(a => a.StartsWith("/p:VerifyExtraction=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:VerifyExtraction=", "false");

	    if (allowed.Any(a => a.StartsWith("/p:CreateNewDatabase=", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/p:CreateNewDatabase=", "true");

	    if (allowed.Any(a => a.StartsWith("/TargetTrustServerCertificate:", StringComparison.CurrentCultureIgnoreCase)))
		    args.SetDefault("/TargetTrustServerCertificate:", "true");
	    
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
    /// <returns></returns>
    public static List<string> BuildExportArguments(this IEnumerable<string> args)
    {
	    var arguments = new List<string>();

	    foreach (var arg in args)
	    {
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');

		    if (ExportOptions.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)))
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Export");
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
    /// <returns></returns>
    public static List<string> BuildImportArguments(this IEnumerable<string> args)
    {
	    var arguments = new List<string>();

	    foreach (var arg in args)
	    {
		    var argPrefix = arg.Split(arg.Contains('=') ? '=' : ':')[0] + (arg.Contains('=') ? '=' : ':');

		    if (ImportOptions.Any(a => a.StartsWith(argPrefix, StringComparison.CurrentCultureIgnoreCase)))
			    arguments.Add(arg);
	    }

	    arguments.Insert(0, "/a:Import");
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

                    filteredArguments.Add(arg.Replace(";", "\\;"));
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