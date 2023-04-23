using System.Diagnostics;
using Argentini.SqlPkg.Extensions;
using CliWrap;
using Microsoft.Data.SqlClient;

namespace Argentini.SqlPkg;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine($"SqlPkg => Started at {DateTime.Now:o}");

        if (await CliHelpers.SqlPackageIsInstalled() == false)
            return -1;
        
        var timer = new Stopwatch();

        #region Debug Test

        args = new[]
        {
            "/Action:Extract",
            "/TargetFile:datoids.dacpac",
            "/DiagnosticsFile:sqlpkg.log",
            "/p:ExtractAllTableData=true",
            "/p:ExcludeTableData=[dbo].[_luceneQueue]",
            "/SourceServerName:sqlserver,1433",
            "/SourceDatabaseName:datoids",
            "/SourceUser:sa",
            "/SourcePassword:P@ssw0rdz!"
        };
        
        #endregion

        timer.Start();
        
        var arguments = new List<string>();
        var tableDataList = new List<string>();
        
        if (args.Length > 1)
        {
            #region Normalize Connection Info
            
            var sourceConnectionString = args.GetArgumentValue("/SourceConnectionString", "/scs", ':');
            var sourceServerName = args.GetArgumentValue("/SourceServerName", "/ssn", ':');
            var sourceDatabaseName = args.GetArgumentValue("/SourceDatabaseName", "/sdn", ':');
            var sourceUserName = args.GetArgumentValue("/SourceUser", "/su", ':');
            var sourcePassword = args.GetArgumentValue("/SourcePassword", "/sp", ':');

            if (string.IsNullOrEmpty(sourceConnectionString) == false)
            {
                var builder = new SqlConnectionStringBuilder(sourceConnectionString)
                {
                    TrustServerCertificate = true,
                    ConnectTimeout = 45,
                    CommandTimeout = 45
                };

                sourceConnectionString = builder.ToString();

                sourceServerName = builder.DataSource;
                sourceUserName = builder.UserID;
                sourcePassword = builder.Password;
                sourceDatabaseName = builder.InitialCatalog;
            }

            else
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = sourceServerName,
                    InitialCatalog = sourceDatabaseName,
                    UserID = sourceUserName,
                    Password = sourcePassword,
                    TrustServerCertificate = true,
                    Authentication = SqlAuthenticationMethod.SqlPassword,
                    ConnectTimeout = 45,
                    CommandTimeout = 45
                };

                sourceConnectionString = builder.ToString();
            }

            #endregion
            
            #region Better Defaults

            args = args.SetDefault("/SourceTrustServerCertificate:", "true");
            args = args.SetDefault("/p:IgnoreUserLoginMappings=", "true");
            args = args.SetDefault("/p:IgnorePermissions=", "true");
            args = args.SetDefault("/p:VerifyExtraction=", "false");

            #endregion
        
            #region Extract
            
            if (args.Any(a => a.StartsWith("/a:extract", StringComparison.CurrentCultureIgnoreCase) || a.StartsWith("/action:extract", StringComparison.CurrentCultureIgnoreCase)))
            {
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

                else if (args.Any(a => a.StartsWith("/p:ExtractAllTableData=true", StringComparison.CurrentCultureIgnoreCase)))
                {
                    tableDataList.AddRange(await SqlTools.LoadTableNames(sourceConnectionString));
                }
            
                if (args.Any(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
                {
                    foreach (var exclusion in args.Where(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var excludedTableName = exclusion.Split('=').Length == 2 ? exclusion.Split('=')[1] : string.Empty;

                        if (string.IsNullOrEmpty(excludedTableName))
                            continue;

                        excludedTableName = excludedTableName.NormalizeTableName();

                        tableDataList.RemoveAll(t => t.Equals(excludedTableName, StringComparison.CurrentCultureIgnoreCase));
                    }
                }
                
                foreach (var arg in args)
                {
                    if (arg.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase) || arg.StartsWith("/p:TableData=", StringComparison.CurrentCultureIgnoreCase) || arg.StartsWith("/p:ExtractAllTableData=true", StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    arguments.Add(arg);
                }

                arguments.AddRange(tableDataList.Select(tableName => $"/p:TableData={tableName}"));
            }

            #endregion
            
            else
            {
                foreach (var arg in args)
                {
                    arguments.Add(arg);
                }
            }
        }
        
        await using var stdOut = Console.OpenStandardOutput();
        
        var cmd = Cli.Wrap("SqlPackage")
            .WithArguments(arguments)
            .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
            .WithStandardErrorPipe(PipeTarget.ToStream(stdOut));

        var result = await cmd.ExecuteAsync();
        var elapsed = $"{timer.Elapsed:g}";
        var elapsedSplits = elapsed.Split('.');

        if (elapsedSplits is [_, { Length: > 1 }])
            elapsed = $"{elapsedSplits[0]}.{elapsedSplits[1][..2]}";
        
        Console.WriteLine($"SqlPkg => Finished at {DateTime.Now:o}");
        Console.WriteLine($"SqlPkg => Total Elapsed: {elapsed}");
        
        return result.ExitCode;
    }
}
