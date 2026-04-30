using System.CommandLine;
using ArchAudit.Cli.Models;

namespace ArchAudit.Cli.Commands;

/// <summary>
/// The 'init' command: generates a default .archaudit.yml configuration file.
/// </summary>
public sealed class InitCommand : Command
{
    public InitCommand()
        : base("init", "Generate a default .archaudit.yml configuration file")
    {
        var forceOption = new Option<bool>(
            name: "--force",
            description: "Overwrite existing .archaudit.yml if it exists",
            getDefaultValue: () => false);

        AddOption(forceOption);

        this.SetHandler((bool force) =>
        {
            Run(force);
        }, forceOption);
    }

    private static void Run(bool force)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), ".archaudit.yml");

        if (File.Exists(configPath) && !force)
        {
            Console.Error.WriteLine($".archaudit.yml already exists. Use --force to overwrite.");
            Environment.Exit(1);
        }

        var defaults = ArchAuditConfig.CreateDefault();
        var yaml = defaults.ToYaml();

        File.WriteAllText(configPath, yaml);
        Console.Out.WriteLine($"Generated .archaudit.yml at '{configPath}'");
    }
}
