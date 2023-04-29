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

        // args = new[]
        // {
        //     "/a:backup",
        //     "/TargetFile:\"Database Backups/athepedia.bacpac\"",
        //     "/DiagnosticsFile:\"Database Backups/Logs/athepedia.log\"",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog]",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog2]",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog3]",
        //     "/SourceServerName:sqlserver,1433",
        //     "/SourceDatabaseName:athepedia",
        //     "/SourceUser:sa",
        //     "/SourcePassword:'P@ssw0rdz!'"
        // };

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
        
        Console.WriteLine();
        Console.WriteLine("SqlPkg: Back up and restore SQL Server databases with Microsoft SqlPackage.");
        Console.WriteLine($"Version {AppState.Version} for {Identify.GetOsPlatformName()} ({Identify.GetPlatformArchitecture()}); .NET {Identify.GetRuntimeVersion()}");
        Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
        Console.WriteLine();

        if (string.IsNullOrEmpty(AppState.Action) == false)
        {
            Console.Write("Action    ");
            CliOutputHelpers.WriteBar();
            Console.WriteLine($"  {(string.IsNullOrEmpty(AppState.Action) ? "HELP" : AppState.Action)}");
            Console.WriteLine();
        }

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
                    .WithArguments(AppState.WorkingArguments.GetArgumentsStringForCli())
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
                    .WithArguments(AppState.WorkingArguments.GetArgumentsStringForCli())
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
            }
        }

        else if (string.IsNullOrEmpty(AppState.Action) == false)
        {
            Console.Write("Started   ");
            CliOutputHelpers.WriteBar();
            Console.WriteLine("  " + CliOutputHelpers.GetDateTime());
            
            Console.Write(" ".Repeat(10));
            CliOutputHelpers.WriteBar();
            Console.WriteLine("  Backup/Restore Not Used, Passing Control to SqlPackage");
            Console.WriteLine();

            Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
            Console.WriteLine();
            
            AppState.OriginalArguments.EnsureDirectoryExists("/TargetFile:", "/df:");
            AppState.OriginalArguments.EnsureDirectoryExists("/DiagnosticsFile:", "/df:");
            
            AppState.OriginalArguments.WrapPathsInQuotes();
            
            var cmd = Cli.Wrap("SqlPackage")
                .WithArguments(AppState.OriginalArguments.GetArgumentsStringForCli())
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

            var result = await cmd.ExecuteAsync();

            resultCode = result.ExitCode;
        }

        else
        {
            const string helpText = @"
SqlPkg can be used in 'Backup' or 'Restore' action modes, which are functionally equivalent to SqlPackage's 'Export' and 'Import' action modes. These modes have tailored default values and provide additional features.

/Action:Backup (/a:Backup)
    Accepts all Action:Export arguments, and also provides /ExcludeTableData: which is functionally equivalent to /TableData: but excludes the specified tables. Can be listed multiple times to exclude multiple tables.

/Action:Restore (/a:Restore)
    Accepts all Action:Import arguments. This mode will always fully erase the target database prior to restoring the .bacpac file, so there's no need to create a new database each time.

For convenience, you can also use SqlPkg in place of SqlPackage for all other operations as all arguments are passed through.";

            helpText.WriteToConsole(ApplicationState.ColumnWidth);
            Console.WriteLine();
            Console.WriteLine("▬".Repeat(ApplicationState.ColumnWidth));
            Console.WriteLine();

            if (OperatingSystem.IsWindows())
            {
                // BEGIN: Workaround for Windows 11 Bug with SqlPackage Help Mode

                var p = new Process();

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = "sqlpackage.exe";
                p.Start();

                await p.WaitForExitAsync();

                resultCode = p.ExitCode;

                // END: Workaround for Windows 11 Bug with SqlPackage Help Mode
            }

            else
            {
                var cmd = Cli.Wrap("SqlPackage")
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
            }
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

        Console.WriteLine($"{AppState.Action.ToUpper()} COMPLETE on {CliOutputHelpers.GetDateTime()}");
        Console.WriteLine();

        CliOutputHelpers.OutputCompleteInfo(AppState);

        Console.Write("Elapsed   ");
        CliOutputHelpers.WriteBar();
        Console.WriteLine($"  {elapsed}");
        Console.WriteLine();

        return resultCode;
    }
}
