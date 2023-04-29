using System.Diagnostics;
using Argentini.SqlPkg.Extensions;

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

        //args = new[]
        //{
        //     "/a:backup",
        //     "/TargetFile:\"Database Backups/AdventureWorks2019.bacpac\"",
        //     "/DiagnosticsFile:\"Database Backups/Logs/AdventureWorks2019.log\"",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog]",
        //     "/SourceServerName:sqlserver,1433",
        //     "/SourceDatabaseName:AdventureWorks2019",
        //     "/SourceUser:sa",
        //     "/SourcePassword:'P@ssw0rdz!'"
        // };

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
        
        Console.WriteLine();
        Console.WriteLine("SqlPkg: Back up and restore SQL Server databases with Microsoft SqlPackage.");
        Console.WriteLine($"Version {AppState.Version} for {Identify.GetOsPlatformName()} ({Identify.GetPlatformArchitecture()}); .NET {Identify.GetRuntimeVersion()}");
        Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
        Console.WriteLine();

        if (string.IsNullOrEmpty(AppState.Action) == false)
        {
            Console.Write("Action    ");
            CliHelpers.WriteBar();
            Console.WriteLine($"  {(string.IsNullOrEmpty(AppState.Action) ? "HELP" : AppState.Action)}");
            Console.WriteLine();
        }

        if (await ApplicationState.SqlPackageIsInstalled() == false)
            return -1;
        
        if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase) || AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.Write("Started   ");
            CliHelpers.WriteBar();
            Console.WriteLine("  " + CliHelpers.GetDateTime());
            Console.WriteLine();
            
            if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            {
                CliHelpers.OutputBackupInfo(AppState);
                
                Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
                Console.WriteLine();
                
                AppState.BuildBackupArguments();
                
                await AppState.ProcessTableDataArguments();

                resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.WorkingArguments.GetArgumentsStringForCli());
            }

            else if (AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            {
                CliHelpers.OutputRestoreInfo(AppState);

                Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
                Console.WriteLine();
                
                AppState.BuildRestoreArguments();
                
                await SqlTools.PurgeOrCreateDatabaseAsync(AppState);

                resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.WorkingArguments.GetArgumentsStringForCli());
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

            Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
            Console.WriteLine();
            
            AppState.OriginalArguments.EnsureDirectoryExists("/TargetFile:", "/df:");
            AppState.OriginalArguments.EnsureDirectoryExists("/DiagnosticsFile:", "/df:");
            
            AppState.OriginalArguments.WrapPathsInQuotes();
            
            resultCode = await CliHelpers.ExecuteSqlPackageAsync(AppState.OriginalArguments.GetArgumentsStringForCli());
        }

        else
        {
            const string helpText = @"
SqlPkg can be used in 'Backup' or 'Restore' action modes, which are functionally equivalent to SqlPackage's 'Export' and 'Import' action modes. These modes have tailored default values and provide additional features.

/Action:Backup (/a:Backup)
    Accepts all Action:Export arguments, and also provides /ExcludeTableData: which is functionally equivalent to /TableData: but excludes the specified tables. Can be listed multiple times to exclude multiple tables.

/Action:Restore (/a:Restore)
    Accepts all Action:Import arguments. This mode will always fully erase the target database or create a new database if none is found, prior to restoring the .bacpac file.

For convenience, you can also use SqlPkg in place of SqlPackage for all other operations as all arguments are passed through.";

            helpText.WriteToConsole(ApplicationState.ColumnWidth);
            Console.WriteLine();
            Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
            Console.WriteLine();

            resultCode = await CliHelpers.ExecuteSqlPackageAsync();
        }

        if (string.IsNullOrEmpty(AppState.Action))
            return resultCode;
        
        var elapsed = $"{timer.Elapsed:g}";
        var elapsedSplits = elapsed.Split('.');

        if (elapsedSplits is [_, { Length: > 1 }])
            elapsed = $"{elapsedSplits[0]}.{elapsedSplits[1][..2]}";

        Console.WriteLine();
        Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
        Console.WriteLine();

        Console.WriteLine($"{AppState.Action.ToUpper()} COMPLETE on {CliHelpers.GetDateTime()}");
        Console.WriteLine();

        CliHelpers.OutputCompleteInfo(AppState);

        Console.Write("Elapsed   ");
        CliHelpers.WriteBar();
        Console.WriteLine($"  {elapsed}");
        Console.WriteLine();

        return resultCode;
    }
}
