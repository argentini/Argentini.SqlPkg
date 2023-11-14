using System.Text;

namespace Argentini.SqlPkg.Extensions;

public static class ArgumentHelpers
{
    #region Constants
    
    public static IEnumerable<string> ExportSkippedArguments => new []
    {
        "/Action:",
        "/a:",
        "/TargetFile:",
        "/tf:",
        "/DiagnosticsFile:",
        "/df:",
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
        "/SourceFile:",
        "/sf:",
        "/DiagnosticsFile:",
        "/df:",
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
    
    #endregion

    /// <summary>
    /// Set a default CLI argument value if it doesn't exist.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="argumentName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static void SetDefault(this List<CliArgument> args, string argumentName, string defaultValue = "")
    {
        if (args.HasArgument(argumentName) == false)
            return;
        
        args.Add(new CliArgument
        {
            Key = argumentName,
            Value = defaultValue
        });
    }
    
    /// <summary>
    /// Determine if an argument has been passed on the command line.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="argumentName"></param>
    /// <param name="argumentNameAbbrev"></param>
    /// <returns></returns>
    public static bool HasArgument(this List<CliArgument> arguments, string argumentName, string argumentNameAbbrev = "")
    {
        if (arguments.Any() == false)
            return false;

        return arguments.Any(a =>
            a.Key.Equals(argumentName, StringComparison.CurrentCultureIgnoreCase) ||
            (string.IsNullOrEmpty(argumentNameAbbrev) == false && a.Key.Equals(argumentNameAbbrev, StringComparison.CurrentCultureIgnoreCase)));
    }
    
    /// <summary>
    /// Get a CLI argument value, or a default value if not found.
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="argumentName"></param>
    /// <param name="argumentNameAbbrev"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string GetArgumentValue(this List<CliArgument> arguments, string argumentName, string argumentNameAbbrev, string defaultValue = "")
    {
        if (arguments.Any() == false)
            return defaultValue;

        if (arguments.HasArgument(argumentName, argumentNameAbbrev) == false)
            return defaultValue;
        
        return arguments.First(a =>
            a.Key.Equals(argumentName, StringComparison.CurrentCultureIgnoreCase) ||
            (string.IsNullOrEmpty(argumentNameAbbrev) == false && a.Key.Equals(argumentNameAbbrev, StringComparison.CurrentCultureIgnoreCase))).Value;
    }

    /// <summary>
    /// Ensure the directory path exists for a given argument file path.
    /// </summary>
    /// <param name="filePath"></param>
    public static void EnsureDirectoryExists(this string filePath)
    {
        if (filePath.Contains(Path.DirectorySeparatorChar) == false && filePath.Contains(Path.AltDirectorySeparatorChar) == false)
            return;

        var directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        
        if (string.IsNullOrEmpty(directoryPath) == false && Directory.Exists(directoryPath) == false)
            Directory.CreateDirectory(directoryPath);
    }
    
    /// <summary>
    /// Assemble the WorkingArguments into a string for the CLI.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static string GetArgumentsStringForCli(this List<CliArgument> arguments)
    {
        var result = new StringBuilder();

        foreach (var argument in arguments)
        {
            result.Append(argument.Key);
            result.Append(argument.Value);
            result.Append(' ');
        }

        return result.ToString().TrimEnd();
    }

    /// <summary>
    /// Assemble the WorkingArguments into a string array for the CLI.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static List<string> GetArgumentsForCli(this List<CliArgument> arguments)
    {
        var result = new List<string>();

        foreach (var argument in arguments)
        {
            result.Add($"{argument.Key}{argument.Value}");
        }

        return result;
    }
    
    /// <summary>
    /// Change the filename portion of a file path.
    /// If just a filename, it returns the new filename.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="newFileName"></param>
    /// <returns></returns>
    public static string ChangeFileNameInPath(this string filePath, string newFileName)
    {
        if (string.IsNullOrEmpty(filePath))
            return newFileName;

        if (filePath.Contains(Path.DirectorySeparatorChar) == false && filePath.Contains(Path.AltDirectorySeparatorChar) == false)
            return newFileName;
        
        var separator = filePath.Contains(Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : Path.AltDirectorySeparatorChar;
        var path = filePath[..filePath.LastIndexOf(separator)];
        
        return Path.Combine(path, newFileName);        
    }
}