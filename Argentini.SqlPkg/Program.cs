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
        var schemaElapsed = string.Empty;
        var dataElapsed = string.Empty;
        var resultCode = 0;
        
        await using var stdOut = Console.OpenStandardOutput();

        var title = $"SqlPkg for SqlPackage {version}; {CliHelpers.GetOsPlatformName()} ({CliHelpers.GetPlatformArchitecture()}); CLR {CliHelpers.GetRuntimeVersion()}";
        
        Console.WriteLine(title);
        Console.WriteLine("=".Repeat(title.Length));

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
            Console.WriteLine($"SqlPkg => {settings.Action} Started {DateTime.Now:o}");
            
            args.NormalizeConnectionInfo(settings);

            if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("SqlPkg => Back Up Started...");

                #region Backup Schema as DACPAC

                var taskTimer = new Stopwatch();
                    
                taskTimer.Start();

                var schemaArguments = args.BuildExportArguments();
                
                await schemaArguments.ProcessTableDataArguments(args, settings);
                
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(string.Join(" ", schemaArguments))
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;

                schemaElapsed = $"{taskTimer.Elapsed:g}";
                var taskElapsedSplits = schemaElapsed.Split('.');

                if (taskElapsedSplits is [_, { Length: > 1 }])
                    schemaElapsed = $"{taskElapsedSplits[0]}.{taskElapsedSplits[1][..2]}";
                
                Console.WriteLine($"SqlPkg => Backup Complete: {schemaElapsed}");
                
                #endregion
            }

            else if (settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("SqlPkg => Restoring Database...");

                #region Restore Schema from DACPAC

                var taskTimer = new Stopwatch();
                    
                taskTimer.Start();

                var schemaArguments = args.BuildImportArguments();
                
                await SqlTools.PurgeDatabase(settings);
                
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(string.Join(" ", schemaArguments))
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;

                schemaElapsed = $"{taskTimer.Elapsed:g}";
                var taskElapsedSplits = schemaElapsed.Split('.');

                if (taskElapsedSplits is [_, { Length: > 1 }])
                    schemaElapsed = $"{taskElapsedSplits[0]}.{taskElapsedSplits[1][..2]}";
                
                Console.WriteLine($"SqlPkg => Restoration Complete: {schemaElapsed}");
                
                #endregion
            }
        }

        else
        {
            Console.WriteLine("SqlPkg => Backup/Restore Not Used => Passing Control to SqlPackage");
            Console.WriteLine($"SqlPkg => {(settings.Action != string.Empty ? settings.Action + " " : string.Empty)}Started {DateTime.Now:o}");
            
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
        Console.WriteLine($"SqlPkg => {(settings.Action != string.Empty ? settings.Action + " " : string.Empty)}Finished {DateTime.Now:o}");
        Console.WriteLine($"SqlPkg => Schema Time: {schemaElapsed}");
        Console.WriteLine($"SqlPkg => Data Time: {dataElapsed}");
        Console.WriteLine($"SqlPkg => Total Time: {elapsed}");
        
        return resultCode;
    }
}
