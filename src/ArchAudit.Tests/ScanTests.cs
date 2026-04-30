using ArchAudit.Cli.Models;
using ArchAudit.Cli.Services;
using Xunit;

namespace ArchAudit.Tests;

public sealed class ScanTests
{
    [Fact]
    public void Scanner_Finds_Csproj_Files()
    {
        // Arrange: use the test project's own directory (has a .csproj)
        var scanner = new SolutionScanner();

        // Act
        var projects = scanner.Scan(".");

        // Assert
        Assert.NotEmpty(projects);
        Assert.Contains(projects, p => p.Name.Contains("ArchAudit.Tests"));
    }

    [Fact]
    public void Scanner_Parses_Project_References()
    {
        var scanner = new SolutionScanner();
        var projects = scanner.Scan(".");

        // The test project references ArchAudit.Cli
        var testProject = projects.FirstOrDefault(p => p.Name.Contains("ArchAudit.Tests"));
        Assert.NotNull(testProject);
        Assert.Contains(testProject!.References, r => r.Contains("ArchAudit.Cli"));
    }
}
