using System.CommandLine;
using ArchAudit.Cli.Commands;

namespace ArchAudit.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Architecture governance for .NET solutions");

        // Version option
        var versionOption = new Option<bool>("--version", "Show version information");
        rootCommand.AddGlobalOption(versionOption);

        rootCommand.AddCommand(new AuditCommand());
        rootCommand.AddCommand(new InitCommand());

        rootCommand.SetHandler((bool version) =>
        {
            if (version)
            {
                var assembly = typeof(Program).Assembly;
                var versionAttr = assembly.GetCustomAttributes(false)
                    .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault();
                Console.Out.WriteLine(versionAttr?.InformationalVersion ?? "1.0.0");
            }
            else
            {
                Console.Error.WriteLine("Use 'arch-audit audit' to run an audit, or 'arch-audit init' to create a config.");
            }
        }, versionOption);

        return await rootCommand.InvokeAsync(args);
    }
}
