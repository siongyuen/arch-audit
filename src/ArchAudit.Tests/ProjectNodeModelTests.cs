using ArchAudit.Cli.Models;
using Xunit;

namespace ArchAudit.Tests;

public sealed class ProjectNodeModelTests
{
    [Fact]
    public void Constructor_Initializes_Empty_References_And_PackageReferences()
    {
        var project = new ProjectNode();

        Assert.NotNull(project.References);
        Assert.Empty(project.References);

        Assert.NotNull(project.PackageReferences);
        Assert.Empty(project.PackageReferences);
    }

    [Fact]
    public void ToString_Returns_The_Project_Name()
    {
        var project = new ProjectNode { Name = "MyApp.API" };

        Assert.Equal("MyApp.API", project.ToString());
    }

    [Fact]
    public void All_Properties_Set_Correctly()
    {
        var project = new ProjectNode
        {
            Name = "MyApp.Data",
            FilePath = "/src/MyApp.Data/MyApp.Data.csproj",
            Directory = "/src/MyApp.Data",
            References = ["MyApp.Domain"],
            PackageReferences = ["Dapper", "Microsoft.Data.SqlClient"],
        };

        Assert.Equal("MyApp.Data", project.Name);
        Assert.Equal("/src/MyApp.Data/MyApp.Data.csproj", project.FilePath);
        Assert.Equal("/src/MyApp.Data", project.Directory);
        Assert.Single(project.References);
        Assert.Contains("MyApp.Domain", project.References);
        Assert.Equal(2, project.PackageReferences.Count);
        Assert.Contains("Dapper", project.PackageReferences);
        Assert.Contains("Microsoft.Data.SqlClient", project.PackageReferences);
    }

    [Fact]
    public void References_Can_Be_Modified_After_Creation()
    {
        var project = new ProjectNode { Name = "Test" };

        project.References.Add("DepA");
        project.References.Add("DepB");

        Assert.Equal(2, project.References.Count);
        Assert.Contains("DepA", project.References);
        Assert.Contains("DepB", project.References);
    }

    [Fact]
    public void PackageReferences_Can_Be_Modified_After_Creation()
    {
        var project = new ProjectNode { Name = "Test" };

        project.PackageReferences.Add("Newtonsoft.Json");

        Assert.Single(project.PackageReferences);
        Assert.Contains("Newtonsoft.Json", project.PackageReferences);
    }
}