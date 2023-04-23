using System.Diagnostics;
using Argentini.SqlPkg.Extensions;
using CliWrap;

namespace Argentini.SqlPkg;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        const string version = "1.0.1"; // Single file executables can't get the Assembly version so use this
        var settings = new Settings();
        var schemaElapsed = string.Empty;
        var dataElapsed = string.Empty;
        var resultCode = 0;
        
        await using var stdOut = Console.OpenStandardOutput();

        #region Backup Debug Test

        // args = new[]
        // {
        //     "/Action:Backup",
        //     "/TargetFile:\"Database/bullhorn\"",
        //     "/DiagnosticsFile:\"Database/bullhorn.log\"",
        //     "/p:ExcludeTableData=[Bullhorn1].[devTemp_*]",
        //     "/p:ExcludeTableData=[Bullhorn1].[devTmp_*]",
        //     "/p:ExcludeTableData=[Bullhorn1].[dbaTmp_*]",
        //     "/p:ExcludeTableData=[dbo].[temp_*]",
        //     "/SourceServerName:sqlserver,1433",
        //     "/SourceDatabaseName:bullhorn",
        //     "/SourceUser:sa",
        //     "/SourcePassword:P@ssw0rdz!"
        // };
        
        #endregion
        
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
        }

        else
        {
            Console.WriteLine("SqlPkg => Backup/Restore Not Used => Passing Control to SqlPackage");
            Console.WriteLine($"SqlPkg => {(settings.Action != string.Empty ? settings.Action + " " : string.Empty)}Started {DateTime.Now:o}");
        }
        
        #endregion
        
        var tableDataList = new List<string>();
        
        if (args.Length > 1)
        {
            if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase) || settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
            {
                args.NormalizeConnectionInfo(settings);

                if (settings.Action.Equals("Backup", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("SqlPkg => Processing Schema...");

                    #region Backup Schema as DACPAC

                    var taskTimer = new Stopwatch();
                        
                    taskTimer.Start();

                    var schemaArguments = args.BuildSchemaBackupArguments();
                    
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
                    
                    Console.WriteLine($"SqlPkg => Schema Complete: {schemaElapsed}");
                    
                    #endregion
                    
                    if (resultCode > -1)
                    {
                        #region Backup Data as BACPAC

                        Console.WriteLine("SqlPkg => Processing Data...");

                        taskTimer.Restart();
                        
                        var dataArguments = args.BuildDataBackupArguments();

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

                                var additions = new List<string>();
                                
                                foreach (var arg in dataArguments)
                                {
                                    if (arg.StartsWith("/p:TableData=", StringComparison.CurrentCultureIgnoreCase) || arg.StartsWith("/p:ExtractAllTableData=true", StringComparison.CurrentCultureIgnoreCase))
                                        continue;

                                    additions.Add(arg.Replace(";", "\\;"));
                                }

                                dataArguments.Clear();
                                dataArguments.AddRange(additions);
                                dataArguments.AddRange(tableDataList.Select(tableName => $"/p:TableData={tableName}"));
                            }

                            #endregion
                        }

                        cmd = Cli.Wrap("SqlPackage")
                            .WithArguments(string.Join(" ", dataArguments))
                            .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                            .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                        result = await cmd.ExecuteAsync();

                        resultCode = result.ExitCode;

                        dataElapsed = $"{taskTimer.Elapsed:g}";
                        taskElapsedSplits = dataElapsed.Split('.');

                        if (taskElapsedSplits is [_, { Length: > 1 }])
                            dataElapsed = $"{taskElapsedSplits[0]}.{taskElapsedSplits[1][..2]}";
                    
                        Console.WriteLine($"SqlPkg => Data Complete: {dataElapsed}");
                        
                        #endregion
                    }
                }

                else if (settings.Action.Equals("Restore", StringComparison.CurrentCultureIgnoreCase))
                {
                    // TODO: Optionally recreate target
                    
                    // TODO: Restore schema using dacpac
                    
                    // TODO: Restore data using bacpac
                }
            }

            else
            {
                var cmd = Cli.Wrap("SqlPackage")
                    .WithArguments(string.Join(" ", args))
                    .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                    .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

                var result = await cmd.ExecuteAsync();

                resultCode = result.ExitCode;
            }
        }

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
