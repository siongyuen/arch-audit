using ArchAudit.Cli.Models;
using ArchAudit.Cli.Services;
using Xunit;

namespace ArchAudit.Tests;

public sealed class ConfigLoaderTests : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    public ConfigLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void Loads_Config_From_Real_ArchAuditYml()
    {
        // Arrange: create a .archaudit.yml in the temp directory
        var yamlContent = """
            layer:
              forbidden_refs:
                - from: "*.UI"
                  to: "*.Data"
            circular_deps:
              enabled: false
            naming:
              src_directory_only: false
            coupling:
              max_direct_refs: 10
            """;
        File.WriteAllText(Path.Combine(_tempDir, ".archaudit.yml"), yamlContent);

        var loader = new ConfigLoader();

        // Act
        var config = loader.Load(_tempDir);

        // Assert
        Assert.NotNull(config);
        Assert.NotNull(config.Layer);
        Assert.Single(config.Layer!.ForbiddenRefs);
        Assert.Equal("*.UI", config.Layer.ForbiddenRefs[0].From);
        Assert.Equal("*.Data", config.Layer.ForbiddenRefs[0].To);

        Assert.NotNull(config.CircularDeps);
        Assert.False(config.CircularDeps!.Enabled);

        Assert.NotNull(config.Naming);
        Assert.False(config.Naming!.SrcDirectoryOnly);

        Assert.NotNull(config.Coupling);
        Assert.Equal(10, config.Coupling!.MaxDirectRefs);
    }

    [Fact]
    public void Returns_Defaults_When_No_Config_File_Exists()
    {
        // Arrange: temp dir has no .archaudit.yml
        var loader = new ConfigLoader();

        // Act
        var config = loader.Load(_tempDir);

        // Assert: should return defaults
        Assert.NotNull(config);
        Assert.NotNull(config.Layer);
        Assert.NotEmpty(config.Layer!.ForbiddenRefs);

        Assert.NotNull(config.CircularDeps);
        Assert.True(config.CircularDeps!.Enabled);

        Assert.NotNull(config.Naming);
        Assert.True(config.Naming!.SrcDirectoryOnly);

        Assert.NotNull(config.Coupling);
        Assert.Equal(5, config.Coupling!.MaxDirectRefs);
    }

    [Fact]
    public void Returns_Defaults_When_Config_File_Is_Malformed()
    {
        // Arrange: create a malformed .archaudit.yml
        File.WriteAllText(Path.Combine(_tempDir, ".archaudit.yml"), "::: invalid yaml {{{");

        var loader = new ConfigLoader();

        // Act — the ConfigLoader catches exceptions and falls back to defaults
        var config = loader.Load(_tempDir);

        // Assert: should return defaults (the loader catches YamlDotNet exceptions)
        Assert.NotNull(config);
        Assert.NotNull(config.Layer);
        Assert.NotEmpty(config.Layer!.ForbiddenRefs);

        Assert.NotNull(config.CircularDeps);
        Assert.True(config.CircularDeps!.Enabled);

        Assert.NotNull(config.Naming);
        Assert.True(config.Naming!.SrcDirectoryOnly);

        Assert.NotNull(config.Coupling);
        Assert.Equal(5, config.Coupling!.MaxDirectRefs);
    }

    [Fact]
    public void MergeWithDefaults_Fills_In_Missing_Sections()
    {
        // Arrange: create a config with only the layer section
        var yamlContent = """
            layer:
              forbidden_refs: []
            """;
        File.WriteAllText(Path.Combine(_tempDir, ".archaudit.yml"), yamlContent);

        var loader = new ConfigLoader();

        // Act
        var config = loader.Load(_tempDir);

        // Assert: layer was loaded from file (empty list), others come from defaults
        Assert.NotNull(config.Layer);
        Assert.Empty(config.Layer!.ForbiddenRefs);

        // These should be filled in from defaults
        Assert.NotNull(config.CircularDeps);
        Assert.True(config.CircularDeps!.Enabled);

        Assert.NotNull(config.Naming);
        Assert.True(config.Naming!.SrcDirectoryOnly);

        Assert.NotNull(config.Coupling);
        Assert.Equal(5, config.Coupling!.MaxDirectRefs);
    }
}
