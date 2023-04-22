// See https://aka.ms/new-console-template for more information

using System.Text;
using CliWrap;

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

            await using var stdOut = Console.OpenStandardOutput();
            
            cmd = Cli.Wrap("SqlPackage")
                .WithArguments(args)
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut));

            var result = await cmd.ExecuteAsync();

            return result.ExitCode;
        }

        catch
        {
            Console.WriteLine("SqlPkg => Could not execute the 'SqlPackage' command.");
            Console.WriteLine("Be sure to install it using \"dotnet tool install -g microsoft.sqlpackage\".");
            Console.WriteLine("You will need the dotnet tool (version 6 or later) installed from \"https://dotnet.microsoft.com\" in order to install Microsoft SqlPackage.");
            
            return -1;
        }
    }
}
