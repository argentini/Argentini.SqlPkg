using System.Diagnostics;
using Argentini.SqlPkg.Extensions;
using CliWrap;

namespace Argentini.SqlPkg;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        var settings = new Settings();
        var resultCode = 0;
        
        await using var stdOut = Console.OpenStandardOutput();

        #region Backup Debug Test

        // args = new[]
        // {
        //     "/a:backup",
        //     "/TargetFile:\"Database/athepedia.bacpac\"",
        //     "/DiagnosticsFile:\"Database/athepedia.log\"",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog]",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog2]",
        //     "/p:ExcludeTableData=[dbo].[umbracoLog3]",
        //     "/SourceServerName:sqlserver,1433",
        //     "/SourceDatabaseName:athepedia",
        //     "/SourceUser:sa",
        //     "/SourcePassword:'P@ssw0rdz!'"
        // };

        args = new[]
        {
            "/a:Restore",
            "/SourceFile:\"Database/athepedia.bacpac\"",
            "/DiagnosticsFile:\"Database/athepedia.log\"",
            "/TargetServerName:10.1.10.3,1433",
            "/TargetDatabaseName:temp",
            "/TargetUser:sa",
            "/TargetPassword:P@ssw0rdz!",
            //"/p:ExcludeObjectTypes=Filegroups;Files;FileTables;PartitionFunctions;PartitionSchemes;ServerTriggers;DatabaseTriggers"
        };
        
        #endregion
        
        if (await CliHelpers.SqlPackageIsInstalled() == false)
            return -1;

        var timer = new Stopwatch();

        timer.Start();
        
        #region Get Action

        if (args.Length > 1 && args.Any(a => a.StartsWith("/a:", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith("/action:", StringComparison.CurrentCultureIgnoreCase)))
        {
            var splits = args.First(a =>
                a.StartsWith("/a:", StringComparison.CurrentCultureIgnoreCase) ||
                a.StartsWith("/action:", StringComparison.CurrentCultureIgnoreCase)).Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length == 2)
            {
                settings.Action = splits[1].ApTitleCase();
            }
        }

        #endregion
        
        var title = $"SQLPKG for SqlPackage {Settings.Version}{{{{gap}}}}— {(string.IsNullOrEmpty(settings.Action) ? "HELP" : settings.Action.ToUpper())} MODE —{{{{gap}}}}{Identify.GetOsPlatformName()} ({Identify.GetPlatformArchitecture()}); CLR {Identify.GetRuntimeVersion()}".FillWidth(Settings.ColumnWidth);
        
        Console.WriteLine(title);
        Console.WriteLine(CliHelpers.GetHeaderBar().Repeat(Settings.ColumnWidth));
        Console.WriteLine();
        
        if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase) || settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.Write("Started      ");
            CliHelpers.WriteBar();
            Console.WriteLine("  " + CliHelpers.GetDateTime());
            Console.WriteLine();
            
            args.NormalizeConnectionInfo(settings);

            if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            {
                CliHelpers.OutputBackupInfo(args, settings);
                
                Console.WriteLine("▬".Repeat(Settings.ColumnWidth));
                Console.WriteLine();
                
                #region Backup Database as BACPAC

                var backupArguments = args.BuildExportArguments(settings);
                
                await backupArguments.ProcessTableDataArguments(args, settings);
                
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(string.Join(" ", backupArguments))
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
                
                #endregion
            }

            else if (settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            {
                CliHelpers.OutputRestoreInfo(args, settings);

                Console.WriteLine("▬".Repeat(Settings.ColumnWidth));
                Console.WriteLine();
                
                #region Restore Database from BACPAC
                
                var restoreArguments = args.BuildImportArguments(settings);
                
                await SqlTools.PurgeDatabase(settings);
                
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(string.Join(" ", restoreArguments))
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
                
                #endregion
            }
        }

        else if (string.IsNullOrEmpty(settings.Action) == false)
        {
            Console.Write("Started      ");
            CliHelpers.WriteBar();
            Console.WriteLine("  " + CliHelpers.GetDateTime());
            
            Console.Write(" ".Repeat(13));
            CliHelpers.WriteBar();
            Console.WriteLine($"  Backup/Restore Not Used, Passing Control to SqlPackage{CliHelpers.Ellipsis}");
            Console.WriteLine();

            Console.WriteLine("▬".Repeat(Settings.ColumnWidth));
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
            Console.WriteLine("▬".Repeat(Settings.ColumnWidth));
            Console.WriteLine();
            
            var cmd = Cli.Wrap("SqlPackage")
                .WithArguments(string.Join(" ", args))
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

            var result = await cmd.ExecuteAsync();

            resultCode = result.ExitCode;
        }

        if (string.IsNullOrEmpty(settings.Action) == false)
        {
            var elapsed = $"{timer.Elapsed:g}";
            var elapsedSplits = elapsed.Split('.');

            if (elapsedSplits is [_, { Length: > 1 }])
                elapsed = $"{elapsedSplits[0]}.{elapsedSplits[1][..2]}";

            Console.WriteLine();
            Console.WriteLine("▬".Repeat(Settings.ColumnWidth));
            Console.WriteLine();

            Console.Write("Action       ");
            CliHelpers.WriteBar();
            Console.WriteLine($"  {(string.IsNullOrEmpty(settings.Action) ? "HELP" : settings.Action)}");
            Console.WriteLine();

            if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
                CliHelpers.OutputBackupInfo(args, settings);

            if (settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
                CliHelpers.OutputRestoreInfo(args, settings);

            Console.Write("Completed    ");
            CliHelpers.WriteBar();
            Console.WriteLine("  " + CliHelpers.GetDateTime());
            Console.Write(" ".Repeat(13));
            CliHelpers.WriteBar();
            Console.WriteLine($"  {elapsed} Total Time");
            Console.WriteLine();
        }

        return resultCode;
    }
}
