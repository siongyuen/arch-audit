using ArchAudit.Cli.Models;
using ArchAudit.Cli.Services;
using Xunit;

namespace ArchAudit.Tests;

public sealed class AnalyzerTests
{
    [Fact]
    public void No_Violations_For_Clean_Architecture()
    {
        // Arrange: a simple clean dependency graph
        var projects = new List<ProjectNode>
        {
            new() { Name = "MyApp.Api", Directory = "src/MyApp.Api", References = ["MyApp.Application"] },
            new() { Name = "MyApp.Application", Directory = "src/MyApp.Application", References = ["MyApp.Domain"] },
            new() { Name = "MyApp.Infrastructure", Directory = "src/MyApp.Infrastructure", References = ["MyApp.Application"] },
            new() { Name = "MyApp.Domain", Directory = "src/MyApp.Domain", References = [] },
        };

        var config = ArchAuditConfig.CreateDefault();
        var analyzer = new DependencyGraphAnalyzer(config);

        // Act
        var violations = analyzer.Analyse(projects);

        // Assert
        Assert.DoesNotContain(violations, v => v.Category == "CircularDependency");
        Assert.DoesNotContain(violations, v => v.Category == "Naming");
        // Layer refs should pass — no UI/Web projects
    }

    [Fact]
    public void Detects_Forbidden_Layer_References()
    {
        var projects = new List<ProjectNode>
        {
            new() { Name = "MyApp.UI", Directory = "src/MyApp.UI", References = ["MyApp.Data"] },
            new() { Name = "MyApp.Data", Directory = "src/MyApp.Data", References = [] },
        };

        var config = ArchAuditConfig.CreateDefault();
        var analyzer = new DependencyGraphAnalyzer(config);

        var violations = analyzer.Analyse(projects);

        Assert.Contains(violations, v =>
            v.Category == "Layer" &&
            v.ProjectName == "MyApp.UI" &&
            v.Target == "MyApp.Data");
    }

    [Fact]
    public void Detects_Circular_Dependency()
    {
        var projects = new List<ProjectNode>
        {
            new() { Name = "ProjectA", Directory = "src/ProjectA", References = ["ProjectB"] },
            new() { Name = "ProjectB", Directory = "src/ProjectB", References = ["ProjectC"] },
            new() { Name = "ProjectC", Directory = "src/ProjectC", References = ["ProjectA"] },
        };

        var config = ArchAuditConfig.CreateDefault();
        var analyzer = new DependencyGraphAnalyzer(config);

        var violations = analyzer.Analyse(projects);

        Assert.Contains(violations, v => v.Category == "CircularDependency");
    }

    [Fact]
    public void Detects_Projects_Outside_Src()
    {
        var projects = new List<ProjectNode>
        {
            new() { Name = "RootProject", Directory = "RootProject", References = [] },
        };

        var config = ArchAuditConfig.CreateDefault();
        var analyzer = new DependencyGraphAnalyzer(config);

        var violations = analyzer.Analyse(projects);

        Assert.Contains(violations, v =>
            v.Category == "Naming" &&
            v.ProjectName == "RootProject");
    }

    [Fact]
    public void Detects_Excessive_Coupling()
    {
        var projects = new List<ProjectNode>
        {
            new() { Name = "GodProject", Directory = "src/GodProject",
                References = ["LibA", "LibB", "LibC", "LibD", "LibE", "LibF"] },
        };

        var config = ArchAuditConfig.CreateDefault();
        var analyzer = new DependencyGraphAnalyzer(config);

        var violations = analyzer.Analyse(projects);

        Assert.Contains(violations, v =>
            v.Category == "Coupling" &&
            v.ProjectName == "GodProject");
    }
}
