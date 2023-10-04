using System.Diagnostics;
using Argentini.SqlPkg.Extensions;
using Microsoft.Data.SqlClient;

namespace Argentini.SqlPkg;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        var appInstance = new AppInstance();

        return await appInstance.Run(args);
    }
}

public class AppInstance
{
    public ApplicationState AppState { get; } = new();

    public async Task<int> Run(IEnumerable<string> args)
    {
        var resultCode = 0;

        #region Backup Debug Test

        //args = new[]
        //{
        //     "/?"
        // };

        // args = new[]
        // {
        //      "/a:backup-all",
        //      "/TargetFile:\"Database Backups/master.bacpac\"",
        //      "/DiagnosticsFile:\"Database Backups/Logs/master.log\"",
        //      "/p:ExcludeTableData=[dbo].[umbracoLog]",
        //      "/SourceServerName:sqlserver,1433",
        //      "/SourceDatabaseName:AdventureWorks2019",
        //      "/SourceUser:sa",
        //      "/SourcePassword:'P@ssw0rdz!'"
        //  };

        // args = new[]
        // {
        //      "/a:Restore-All",
        //      "/SourceFile:\"Database Backups/master.bacpac\"",
        //      "/DiagnosticsFile:\"Database Backups/Logs/master.log\"",
        //      "/TargetServerName:sqlserver,1433",
        //      "/TargetDatabaseName:temp",
        //      "/TargetUser:sa",
        //      "/TargetPassword:P@ssw0rdz!"
        //  };
        
        // args = new[]
        // {
        //      "/a:backup",
        //      "/TargetFile:\"Database Backups/AdventureWorks2019.bacpac\"",
        //      "/DiagnosticsFile:\"Database Backups/Logs/AdventureWorks2019.log\"",
        //      "/p:ExcludeTableData=[dbo].[umbracoLog]",
        //      "/SourceServerName:sqlserver,1433",
        //      "/SourceDatabaseName:AdventureWorks2019",
        //      "/SourceUser:sa",
        //      "/SourcePassword:'P@ssw0rdz!'"
        //  };
        
        //args = new[]
        //{
        //     "/a:Restore",
        //     "/SourceFile:\"Database Backups/AdventureWorks2019.bacpac\"",
        //     "/DiagnosticsFile:\"Database Backups/Logs/AdventureWorks2019.log\"",
        //     "/TargetServerName:sqlserver,1433",
        //     "/TargetDatabaseName:temp",
        //     "/TargetUser:sa",
        //     "/TargetPassword:P@ssw0rdz!"
        // };

        #endregion

        var timer = new Stopwatch();

        timer.Start();

        // Parse Arguments

        AppState.ImportArguments(args);
        
        if (await AppState.SqlPackageIsInstalled() == false)
            return -1;

        Console.WriteLine();
        Console.WriteLine("SqlPkg: Back up and restore SQL Server databases with Microsoft SqlPackage.");
        Console.WriteLine($"Version {ApplicationState.Version} for {Identify.GetOsPlatformName()} (.NET {Identify.GetRuntimeVersion()}/{Identify.GetPlatformArchitecture()}); SqlPackage Version {AppState.SqlPackageVersion}");
        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));

        if (string.IsNullOrEmpty(AppState.Action) == false)
        {
            Console.Write("Action    ");
            CliHelpers.WriteBar();
            Console.WriteLine($"  {(string.IsNullOrEmpty(AppState.Action) ? "HELP" : AppState.Action)}");
        }
        
        if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase) || AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase) || AppState.Action.Equals("Backup-All", StringComparison.CurrentCultureIgnoreCase) || AppState.Action.Equals("Restore-All", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.Write("Started   ");
            CliHelpers.WriteBar();
            Console.WriteLine("  " + CliHelpers.GetDateTime());

            if (AppState.Action.Equals("Backup-All", StringComparison.CurrentCultureIgnoreCase))
            {
                var databaseNames = await SqlTools.GetAllDatabaseNamesAsync(AppState);

                if (databaseNames.Any())
                {
                    var counter = 1;
                    
                    foreach (var databaseName in databaseNames)
                    {
                        var builder = new SqlConnectionStringBuilder(AppState.SourceConnectionString)
                        {
                            InitialCatalog = databaseName
                        };

                        AppState.SourceConnectionString = builder.ToString();
                        AppState.SourceDatabaseName = databaseName;
                        AppState.TargetFile = AppState.TargetFile.ChangeFileNameInPath($"{databaseName}.bacpac");

                        if (string.IsNullOrEmpty(AppState.LogFile) == false)
                            AppState.LogFile = AppState.LogFile.ChangeFileNameInPath($"{databaseName}.log");
                        
                        if (databaseName != databaseNames.First())
                            Console.WriteLine();
                        
                        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                        Console.Write("Job       ");
                        CliHelpers.WriteBar();
                        Console.WriteLine($"  {counter++:N0} of {databaseNames.Count:N0}");

                        CliHelpers.OutputBackupInfo(AppState);
                
                        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                        Console.WriteLine();
                
                        AppState.BuildBackupArguments();
                
                        await AppState.ProcessTableDataArguments();

                        resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.WorkingArguments);
                    }
                }

                else
                {
                    Console.Write("Error");
                    CliHelpers.WriteArrow(true);
                    Console.WriteLine("No databases found.");
                    Console.WriteLine();
                }
            }

            else if (AppState.Action.Equals("Restore-All", StringComparison.CurrentCultureIgnoreCase))
            {
                var separator = AppState.SourceFile.Contains(Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar : Path.AltDirectorySeparatorChar;
                var path = string.Empty;
                
                if (AppState.SourceFile.Contains(Path.DirectorySeparatorChar) || AppState.SourceFile.Contains(Path.AltDirectorySeparatorChar))
                    path = AppState.SourceFile[..AppState.SourceFile.LastIndexOf(separator)];

                var files = Directory.GetFiles(path, "*.bacpac").OrderBy(f => f).ToList();

                if (files.Any())
                {
                    var counter = 1;
                    
                    foreach (var file in files)
                    {
                        var separator2 = file.Contains(Path.DirectorySeparatorChar)
                            ? Path.DirectorySeparatorChar
                            : Path.AltDirectorySeparatorChar;
                        var fileName = file[(file.LastIndexOf(separator2) + 1)..];
                        var databaseName = fileName[..fileName.LastIndexOf('.')];

                        var builder = new SqlConnectionStringBuilder(AppState.TargetConnectionString)
                        {
                            InitialCatalog = databaseName
                        };

                        AppState.TargetConnectionString = builder.ToString();
                        AppState.TargetDatabaseName = databaseName;
                        AppState.SourceFile = AppState.SourceFile.ChangeFileNameInPath($"{databaseName}.bacpac");

                        if (string.IsNullOrEmpty(AppState.LogFile) == false)
                            AppState.LogFile = AppState.LogFile.ChangeFileNameInPath($"{databaseName}.log");

                        if (file != files.First())
                            Console.WriteLine();

                        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                        Console.Write("Job       ");
                        CliHelpers.WriteBar();
                        Console.WriteLine($"  {counter++:N0} of {files.Count:N0}");
                        
                        CliHelpers.OutputRestoreInfo(AppState);

                        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                
                        AppState.BuildRestoreArguments();
                
                        await SqlTools.PurgeOrCreateTargetDatabaseAsync(AppState);

                        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                        Console.WriteLine();

                        resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.WorkingArguments);
                    }
                }

                else
                {
                    Console.Write("Error");
                    CliHelpers.WriteArrow(true);
                    Console.WriteLine("No database backups found.");
                    Console.WriteLine();
                }
            }

            else if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            {
                CliHelpers.OutputBackupInfo(AppState);
                
                Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                Console.WriteLine();
                
                AppState.BuildBackupArguments();
                
                await AppState.ProcessTableDataArguments();

                resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.WorkingArguments);
            }

            else if (AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            {
                CliHelpers.OutputRestoreInfo(AppState);

                Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                
                AppState.BuildRestoreArguments();
                
                await SqlTools.PurgeOrCreateTargetDatabaseAsync(AppState);

                Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
                Console.WriteLine();
                
                resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.WorkingArguments);
            }
        }

        else if (string.IsNullOrEmpty(AppState.Action) == false || AppState.OriginalArguments.Count > 0)
        {
            Console.Write("Started   ");
            CliHelpers.WriteBar();
            Console.WriteLine("  " + CliHelpers.GetDateTime());
            
            Console.Write(" ".Repeat(10));
            CliHelpers.WriteBar();
            Console.WriteLine("  Backup/Restore Not Used, Passing Control to SqlPackage");
            Console.WriteLine();

            Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
            Console.WriteLine();
            
            AppState.TargetFile.EnsureDirectoryExists();
            AppState.LogFile.EnsureDirectoryExists();
            
            resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.OriginalArguments);
        }

        else
        {
            const string helpText = @"
SqlPkg can be used in 'Backup' or 'Restore' action modes, which are functionally equivalent to SqlPackage's 'Export' and 'Import' action modes. These modes have tailored default values and provide additional features.

/Action:Backup (/a:Backup)

    Accepts all Action:Export arguments, and also provides /ExcludeTableData: which is functionally equivalent to /TableData: but excludes the specified tables. Can be listed multiple times to exclude multiple tables.

/Action:Restore (/a:Restore)

    Accepts all Action:Import arguments. This mode will always fully erase the target database or create a new database if none is found, prior to restoring the .bacpac file.

/Action:Backup-All (/a:Backup-All)

    This mode will back up all user databases on a server.

    - Provide a source connection to the master database.
    - Provide a target file path ending with 'master.bacpac'. The path will be used as the destination for each database backup file, ignoring 'master.bacpac'.
    - Optionally provide a log file path ending with 'master.log'. The path will be used as the destination for each database backup log file, ignoring 'master.log'.
    - Accepts all arguments that the Backup action mode accepts.

/Action:Restore-All (/a:Restore-All)

    This mode will restore all *.bacpac files in a given path to databases with the same names as the filenames.

    - Provide a source file path to 'master.bacpac' in the location of the bacpac files. The path will be used as the source location for each database backup file to restore, ignoring 'master.bacpac'.
    - Provide a target connection to the master database.
    - Optionally provide a log file path ending with 'master.log'. The path will be used as the destination for each database backup log file, ignoring 'master.log'.
    - Accepts all arguments that the Restore action mode accepts.

For convenience, you can also use SqlPkg in place of SqlPackage for all other operations as all arguments are passed through.";

            Console.WriteLine();
            helpText.WriteToConsole(ApplicationState.ColumnWidth);
            Console.WriteLine();
            Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
            Console.WriteLine();

            resultCode = await CliHelpers.ExecuteSqlPackageAsync(new List<CliArgument>());
        }

        #region Finished...
        
        if (string.IsNullOrEmpty(AppState.Action))
            return resultCode;
        
        var elapsed = $"{timer.Elapsed:g}";
        var elapsedSplits = elapsed.Split('.');

        if (elapsedSplits is [_, { Length: > 1 }])
            elapsed = $"{elapsedSplits[0]}.{elapsedSplits[1][..2]}";

        Console.WriteLine();
        Console.WriteLine("▬".Repeat(ApplicationState.FullColumnWidth));
        Console.WriteLine($"{AppState.Action.ToUpper()} COMPLETE on {CliHelpers.GetDateTime()}");
        Console.WriteLine("-".Repeat(ApplicationState.FullColumnWidth));

        CliHelpers.OutputCompleteInfo(AppState);

        Console.Write("Elapsed   ");
        CliHelpers.WriteBar();
        Console.WriteLine($"  {elapsed}");
        Console.WriteLine();

        return resultCode;
        
        #endregion
    }
}
