using System.Diagnostics;
using Argentini.SqlPkg.Extensions;
using CliWrap;

namespace Argentini.SqlPkg;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        const string version = "1.0.2"; // Single file executables can't get the Assembly version so use this
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
        //     "/SourceServerName:sqlserver,1433",
        //     "/SourceDatabaseName:athepedia",
        //     "/SourceUser:sa",
        //     "/SourcePassword:P@ssw0rdz!"
        // };
        //
        // args = new[]
        // {
        //     "/a:Restore",
        //     "/SourceFile:\"Database/athepedia.bacpac\"",
        //     "/DiagnosticsFile:\"Database/athepedia.log\"",
        //     "/TargetServerName:10.1.10.3,1433",
        //     "/TargetDatabaseName:temp",
        //     "/TargetUser:sa",
        //     "/TargetPassword:P@ssw0rdz!",
        //     "/p:ExcludeObjectTypes=Filegroups;Files;FileTables;PartitionFunctions;PartitionSchemes;ServerTriggers;DatabaseTriggers"
        // };
        
        #endregion
        
        var title = $"SqlPkg for SqlPackage {version}; {CliHelpers.GetOsPlatformName()} ({CliHelpers.GetPlatformArchitecture()}); CLR {CliHelpers.GetRuntimeVersion()}";
        
        Console.WriteLine("-".Repeat(title.Length));
        Console.WriteLine(title);
        Console.WriteLine("-".Repeat(title.Length));

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

        if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase) || settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.WriteLine($"=> {settings.Action} Started {DateTime.Now:o}");
            Console.WriteLine("=".Repeat(title.Length));
            
            args.NormalizeConnectionInfo(settings);

            if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            {
                #region Backup Schema as DACPAC

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
                #region Restore Schema from DACPAC
                
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

        else
        {
            Console.WriteLine("=> Backup/Restore Not Used => Passing Control to SqlPackage");
            Console.WriteLine($"=> {(settings.Action != string.Empty ? settings.Action + " " : string.Empty)}Started {DateTime.Now:o}");
            Console.WriteLine("=".Repeat(title.Length));
            
            var cmd = Cli.Wrap("SqlPackage")
                .WithArguments(string.Join(" ", args))
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

            var result = await cmd.ExecuteAsync();

            resultCode = result.ExitCode;
        }
        
        #endregion
        
        var elapsed = $"{timer.Elapsed:g}";
        var elapsedSplits = elapsed.Split('.');

        if (elapsedSplits is [_, { Length: > 1 }])
            elapsed = $"{elapsedSplits[0]}.{elapsedSplits[1][..2]}";
        
        Console.WriteLine("=".Repeat(title.Length));
        
        if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            Console.WriteLine($"=> Backup of [{settings.SourceDatabaseName}] on {settings.SourceServerName} complete at {DateTime.Now:o}");

        if (settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            Console.WriteLine($"=> Restore to [{settings.TargetDatabaseName}] on {settings.TargetServerName} complete at {DateTime.Now:o}");
        
        Console.WriteLine($"=> Total {(settings.Action != string.Empty ? settings.Action + " " : string.Empty)}Time: {elapsed}");
        
        return resultCode;
    }
}
