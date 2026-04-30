using ArchAudit.Cli.Models;

namespace ArchAudit.Cli.Services;

/// <summary>
/// Loads .archaudit.yml configuration from the specified path or returns defaults.
/// </summary>
public sealed class ConfigLoader
{
    /// <summary>
    /// Loads configuration from the specified directory. Looks for .archaudit.yml in the given path
    /// and walks up directories if not found. Falls back to defaults.
    /// </summary>
    public ArchAuditConfig Load(string? path = null)
    {
        var searchDir = path ?? Directory.GetCurrentDirectory();

        // Walk up from the search directory looking for .archaudit.yml
        var configFile = FindConfigFile(searchDir);
        if (configFile != null)
        {
            try
            {
                var yaml = File.ReadAllText(configFile);
                var config = ArchAuditConfig.FromYaml(yaml);

                // Merge with defaults for any missing sections
                return MergeWithDefaults(config);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Could not load config from '{configFile}': {ex.Message}");
            }
        }

        Console.Error.WriteLine("Info: No .archaudit.yml found, using default rules.");
        return ArchAuditConfig.CreateDefault();
    }

    private static string? FindConfigFile(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            var configPath = Path.Combine(dir.FullName, ".archaudit.yml");
            if (File.Exists(configPath))
                return configPath;

            dir = dir.Parent;
        }

        return null;
    }

    private static ArchAuditConfig MergeWithDefaults(ArchAuditConfig config)
    {
        var defaults = ArchAuditConfig.CreateDefault();

        config.Layer ??= defaults.Layer;
        config.CircularDeps ??= defaults.CircularDeps;
        config.Naming ??= defaults.Naming;
        config.Coupling ??= defaults.Coupling;

        return config;
    }
}
