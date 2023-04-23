// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using Argentini.SqlPkg.Extensions;
using CliWrap;
using Microsoft.Data.SqlClient;

namespace Argentini.SqlPkg;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        var sb = new StringBuilder();
        var cmd = Cli.Wrap("SqlPackage")
            .WithArguments(arguments => { arguments.Add("/version:true"); })
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(sb));

        try
        {
            await cmd.ExecuteAsync();
        }

        catch
        {
            Console.WriteLine("SqlPkg => Could not execute the 'SqlPackage' command.");
            Console.WriteLine("Be sure to install it using \"dotnet tool install -g microsoft.sqlpackage\".");
            Console.WriteLine("You will need the dotnet tool (version 6 or later) installed from \"https://dotnet.microsoft.com\" in order to install Microsoft SqlPackage.");
            
            return -1;
        }
        
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

        Console.WriteLine($"SqlPkg => Started at {DateTime.Now:o}");

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

            if (args.Any(a => a.StartsWith("/SourceTrustServerCertificate:", StringComparison.CurrentCultureIgnoreCase)) == false)
            {
                var tempArgs = args.ToList();

                tempArgs.Add("/SourceTrustServerCertificate:true");
                args = tempArgs.ToArray();
            }

            if (args.Any(a => a.StartsWith("/p:IgnoreUserLoginMappings=", StringComparison.CurrentCultureIgnoreCase)) == false)
            {
                var tempArgs = args.ToList();

                tempArgs.Add("/p:IgnoreUserLoginMappings=true");
                args = tempArgs.ToArray();
            }

            if (args.Any(a => a.StartsWith("/p:IgnorePermissions=", StringComparison.CurrentCultureIgnoreCase)) == false)
            {
                var tempArgs = args.ToList();

                tempArgs.Add("/p:IgnorePermissions=true");
                args = tempArgs.ToArray();
            }

            if (args.Any(a => a.StartsWith("/p:VerifyExtraction=", StringComparison.CurrentCultureIgnoreCase)) == false)
            {
                var tempArgs = args.ToList();

                tempArgs.Add("/p:VerifyExtraction=false");
                args = tempArgs.ToArray();
            }
            
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
                    using (var sqlReader = new SqlReader(new SqlReaderConfiguration
                           {
                               ConnectionString = sourceConnectionString,
                               CommandText = @"
select
schema_name(schema_id) as [SCHEMA_NAME],
[Tables].name as [TABLE_NAME],
[Tables].is_memory_optimized as [TABLE_IS_MEMORY_OPTIMIZED],
[Tables].durability as [TABLE_DURABILITY],
[Tables].durability_desc as [TABLE_DURABILITY_DESC]
from
sys.tables as [Tables]
group by
schema_name(schema_id), [Tables].name, [Tables].is_memory_optimized, [Tables].durability, [Tables].durability_desc
order by
[SCHEMA_NAME] asc, [TABLE_NAME] asc;
"
                           }))
                    {
                        await using (await sqlReader.ExecuteReaderAsync())
                        {
                            if (sqlReader.HasRows)
                            {
                                while (sqlReader.Read())
                                {
                                    var schemaName = await sqlReader.SafeGetStringAsync("SCHEMA_NAME");
                                    var tableName = await sqlReader.SafeGetStringAsync("TABLE_NAME");

                                    tableDataList.Add($"[{schemaName}].[{tableName}]");
                                }
                            }
                        }
                    }
                }
            
                if (args.Any(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
                {
                    foreach (var exclusion in args.Where(a => a.StartsWith("/p:ExcludeTableData=", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var excludedTableName = exclusion.Split('=').Length == 2 ? exclusion.Split('=')[1] : string.Empty;

                        if (string.IsNullOrEmpty(excludedTableName))
                            continue;
                        
                        var splits = excludedTableName.Split('.', StringSplitOptions.RemoveEmptyEntries);

                        if (splits.Length != 2) continue;
                            
                        excludedTableName = $"[{splits[0].Trim('[').Trim(']')}].[{splits[1].Trim('[').Trim(']')}]";
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
        
        cmd = Cli.Wrap("SqlPackage")
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
