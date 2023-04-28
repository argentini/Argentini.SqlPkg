using System.Diagnostics;
using Argentini.SqlPkg.Extensions;
using CliWrap;

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

    public async Task<int> Run(string[] args)
    {
        var resultCode = 0;
        
        await using var stdOut = Console.OpenStandardOutput();

        #region Backup Debug Test

        args = new[]
        {
            "/a:backup",
            "/TargetFile:\"Database Backups/athepedia.bacpac\"",
            "/DiagnosticsFile:\"Database Backups/Logs/athepedia.log\"",
            "/p:ExcludeTableData=[dbo].[umbracoLog]",
            "/p:ExcludeTableData=[dbo].[umbracoLog2]",
            "/p:ExcludeTableData=[dbo].[umbracoLog3]",
            "/SourceServerName:sqlserver,1433",
            "/SourceDatabaseName:athepedia",
            "/SourceUser:sa",
            "/SourcePassword:'P@ssw0rdz!'"
        };

        // args = new[]
        // {
        //     "/a:Restore",
        //     "/SourceFile:\"Database Backups/athepedia.bacpac\"",
        //     "/DiagnosticsFile:\"Database Backups/Logs/athepedia.log\"",
        //     "/TargetServerName:sqlserver,1433",
        //     "/TargetDatabaseName:temp",
        //     "/TargetUser:sa",
        //     "/TargetPassword:P@ssw0rdz!",
        //     //"/p:ExcludeObjectTypes=Filegroups;Files;FileTables;PartitionFunctions;PartitionSchemes;ServerTriggers;DatabaseTriggers"
        // };
        
        #endregion
        
        var timer = new Stopwatch();

        timer.Start();

        // Parse Arguments

        AppState.ImportArguments(args);
        
        // var title = $"SQLPKG for SqlPackage {AppState.Version}{{{{gap}}}}— {(string.IsNullOrEmpty(AppState.Action) ? "HELP" : AppState.Action.ToUpper())} MODE —{{{{gap}}}}{Identify.GetOsPlatformName()} ({Identify.GetPlatformArchitecture()}); CLR {Identify.GetRuntimeVersion()}".FillWidth(ApplicationState.ColumnWidth);
        // Console.WriteLine(title);
        // Console.WriteLine(CliOutputHelpers.GetHeaderBar().Repeat(ApplicationState.ColumnWidth));

        Console.WriteLine();
        Console.WriteLine("SqlPkg: Command-line tool for backing up and restoring SQL Server databases with Microsoft SqlPackage.");
        Console.WriteLine($"Version {AppState.Version} for {Identify.GetOsPlatformName()} ({Identify.GetPlatformArchitecture()}); .NET {Identify.GetRuntimeVersion()}");
        Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
        Console.WriteLine();
        Console.Write("Action    ");
        CliOutputHelpers.WriteBar();
        Console.WriteLine($"  {(string.IsNullOrEmpty(AppState.Action) ? "HELP" : AppState.Action)}");
        Console.WriteLine();

        if (await AppState.SqlPackageIsInstalled() == false)
            return -1;
        
        if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase) || AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.Write("Started   ");
            CliOutputHelpers.WriteBar();
            Console.WriteLine("  " + CliOutputHelpers.GetDateTime());
            Console.WriteLine();
            
            if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            {
                CliOutputHelpers.OutputBackupInfo(AppState);
                
                Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
                Console.WriteLine();
                
                AppState.BuildBackupArguments();
                
                await AppState.ProcessTableDataArguments();
                
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(AppState.GetWorkingArgumentsForCli())
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
            }

            else if (AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            {
                CliOutputHelpers.OutputRestoreInfo(AppState);

                Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
                Console.WriteLine();
                
                AppState.BuildRestoreArguments();
                
                await SqlTools.PurgeOrCreateDatabaseAsync(AppState);
                
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(AppState.GetWorkingArgumentsForCli())
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
            }
        }

        else if (string.IsNullOrEmpty(AppState.Action) == false)
        {
            Console.Write("Started      ");
            CliOutputHelpers.WriteBar();
            Console.WriteLine("  " + CliOutputHelpers.GetDateTime());
            
            Console.Write(" ".Repeat(10));
            CliOutputHelpers.WriteBar();
            Console.WriteLine($"  Backup/Restore Not Used, Passing Control to SqlPackage{CliOutputHelpers.Ellipsis}");
            Console.WriteLine();

            Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
            Console.WriteLine();
            
            var cmd = Cli.Wrap("SqlPackage")
                .WithArguments(string.Join(" ", args))
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

            var result = await cmd.ExecuteAsync();

            resultCode = result.ExitCode;
        }

        else
        {
            Console.WriteLine("SqlPkg can be used in 'Backup' or 'Restore' action modes, which are");
            Console.WriteLine("functionally equivalent to SqlPackage's 'Export' and 'Import' action modes.");
            Console.WriteLine("These modes have tailored default values and provide additional features.");
            Console.WriteLine();
            Console.WriteLine("/Action:Backup (/a:Backup)");
            Console.WriteLine("    Accepts all Action:Export arguments, and also provides /ExcludeTableData:");
            Console.WriteLine("    which is functionally equivalent to /TableData: but excludes the specified");
            Console.WriteLine("    tables. Can be listed multiple times to exclude multiple tables.");
            Console.WriteLine();
            Console.WriteLine("/Action:Restore (/a:Restore)");
            Console.WriteLine("    Accepts all Action:Import arguments. This mode will always fully erase the");
            Console.WriteLine("    target database prior to restoring the .bacpac file, so there's no need to");
            Console.WriteLine("    create a new database each time.");
            Console.WriteLine();
            Console.WriteLine("You can use standard SqlPackage modes and all arguments are sent to Sqlpackage");
            Console.WriteLine("so you can use SqlPkg as the sole way to run SqlPackage, for convenience.");
            Console.WriteLine();
            Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
            Console.WriteLine();
            
            var cmd = Cli.Wrap("SqlPackage")
                .WithArguments(string.Join(" ", args))
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

            var result = await cmd.ExecuteAsync();

            resultCode = result.ExitCode;
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

        Console.Write("Action    ");
        CliOutputHelpers.WriteBar();
        Console.WriteLine($"  {(string.IsNullOrEmpty(AppState.Action) ? "HELP" : AppState.Action)}");
        Console.WriteLine();

        if (AppState.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            CliOutputHelpers.OutputBackupInfo(AppState);

        if (AppState.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            CliOutputHelpers.OutputRestoreInfo(AppState);

        Console.Write("Complete  ");
        CliOutputHelpers.WriteBar();
        Console.WriteLine("  " + CliOutputHelpers.GetDateTime());
        Console.WriteLine();
        
        Console.Write("Elapsed   ");
        CliOutputHelpers.WriteBar();
        Console.WriteLine($"  {elapsed}");
        Console.WriteLine();

        return resultCode;
    }
}
