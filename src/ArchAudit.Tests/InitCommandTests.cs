using System.CommandLine;
using ArchAudit.Cli.Commands;
using ArchAudit.Cli.Models;
using Xunit;

namespace ArchAudit.Tests;

public sealed class InitCommandTests : IDisposable
{
    private readonly string _originalDir;
    private readonly string _tempDir;
    private bool _disposed;

    public InitCommandTests()
    {
        _originalDir = Directory.GetCurrentDirectory();
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        Directory.SetCurrentDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            try
            {
                Directory.SetCurrentDirectory(_originalDir);
            }
            catch { /* ignore if already gone */ }
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Init_Generates_ArchAuditYml_File()
    {
        // Arrange
        var command = new InitCommand();

        // Act — invoke the command's handler (simulate running with --force since we own the dir)
        var fakeArgs = new string[] { "--force" };
        await command.InvokeAsync(fakeArgs);

        // Assert: .archaudit.yml was created
        var configPath = Path.Combine(_tempDir, ".archaudit.yml");
        Assert.True(File.Exists(configPath), "Expected .archaudit.yml to be created");

        // Assert: file has content (valid YAML with expected keys)
        var content = File.ReadAllText(configPath);
        Assert.Contains("layer:", content);
        Assert.Contains("forbidden_refs:", content);
        Assert.Contains("circular_deps:", content);
        Assert.Contains("naming:", content);
        Assert.Contains("coupling:", content);
    }

    [Fact]
    public async Task Init_Does_Not_Overwrite_Existing_File_Without_Force()
    {
        // Arrange: pre-create .archaudit.yml
        var configPath = Path.Combine(_tempDir, ".archaudit.yml");
        File.WriteAllText(configPath, "# existing content");

        var command = new InitCommand();

        // Act — run without --force
        var fakeArgs = Array.Empty<string>();
        var exitCode = await command.InvokeAsync(fakeArgs);

        // Assert: exit code is 1 (error)
        Assert.Equal(1, exitCode);

        // Assert: original file unchanged
        var content = File.ReadAllText(configPath);
        Assert.Equal("# existing content", content.Trim());
    }

    [Fact]
    public async Task Init_Force_Overwrites_Existing_File()
    {
        // Arrange: pre-create .archaudit.yml
        var configPath = Path.Combine(_tempDir, ".archaudit.yml");
        File.WriteAllText(configPath, "# old content");

        var command = new InitCommand();

        // Act — run with --force
        var fakeArgs = new string[] { "--force" };
        var exitCode = await command.InvokeAsync(fakeArgs);

        // Assert: exit code is 0 (success)
        Assert.Equal(0, exitCode);

        // Assert: file was overwritten with fresh config
        var content = File.ReadAllText(configPath);
        Assert.DoesNotContain("# old content", content);
        Assert.Contains("layer:", content);
    }

    [Fact]
    public async Task Init_Yaml_Is_Valid_And_Contains_Default_Values()
    {
        // Arrange
        var command = new InitCommand();
        var configPath = Path.Combine(_tempDir, ".archaudit.yml");

        // Act
        var fakeArgs = new string[] { "--force" };
        await command.InvokeAsync(fakeArgs);

        // Assert: YAML is parseable by ArchAuditConfig
        var yaml = File.ReadAllText(configPath);
        var config = ArchAuditConfig.FromYaml(yaml);

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
    public async Task Init_Without_Force_Exits_With_Code_1_And_Does_Not_Create_File()
    {
        // Arrange: .archaudit.yml already exists
        var configPath = Path.Combine(_tempDir, ".archaudit.yml");
        File.WriteAllText(configPath, "layer:\n  forbidden_refs: []");

        var command = new InitCommand();

        // Act
        var fakeArgs = Array.Empty<string>();
        var exitCode = await command.InvokeAsync(fakeArgs);

        // Assert
        Assert.Equal(1, exitCode);
        // File still exists, unchanged
        Assert.True(File.Exists(configPath));
    }
}