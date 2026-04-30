using ArchAudit.Cli.Models;
using Xunit;

namespace ArchAudit.Tests;

public sealed class ViolationModelTests
{
    [Fact]
    public void All_Properties_Set_Correctly()
    {
        var violation = new Violation
        {
            Severity = ViolationSeverity.Error,
            Category = "Layer",
            Message = "Forbidden reference from UI to Data.",
            ProjectName = "MyApp.UI",
            Target = "MyApp.Data",
        };

        Assert.Equal(ViolationSeverity.Error, violation.Severity);
        Assert.Equal("Layer", violation.Category);
        Assert.Equal("Forbidden reference from UI to Data.", violation.Message);
        Assert.Equal("MyApp.UI", violation.ProjectName);
        Assert.Equal("MyApp.Data", violation.Target);
    }

    [Fact]
    public void ToString_Formats_Correctly()
    {
        var violation = new Violation
        {
            Severity = ViolationSeverity.Warning,
            Category = "Coupling",
            Message = "Too many direct references.",
            ProjectName = "GodProject",
            Target = null,
        };

        var result = violation.ToString();

        Assert.Equal("[Warning] Coupling: Too many direct references.", result);
    }

    [Fact]
    public void ToString_Handles_Null_ProjectName()
    {
        var violation = new Violation
        {
            Severity = ViolationSeverity.Info,
            Category = "Naming",
            Message = "Not in src/.",
            ProjectName = null,
            Target = null,
        };

        var result = violation.ToString();

        Assert.Equal("[Info] Naming: Not in src/.", result);
    }

    [Fact]
    public void ViolationSeverity_Has_All_Expected_Values()
    {
        Assert.True(Enum.IsDefined(typeof(ViolationSeverity), ViolationSeverity.Info));
        Assert.True(Enum.IsDefined(typeof(ViolationSeverity), ViolationSeverity.Warning));
        Assert.True(Enum.IsDefined(typeof(ViolationSeverity), ViolationSeverity.Error));

        // Ensure ordering is stable (used in comparisons/display)
        Assert.Equal(0, (int)ViolationSeverity.Info);
        Assert.Equal(1, (int)ViolationSeverity.Warning);
        Assert.Equal(2, (int)ViolationSeverity.Error);
    }

    [Fact]
    public void Default_Severity_Is_Error()
    {
        var violation = new Violation();

        Assert.Equal(ViolationSeverity.Error, violation.Severity);
    }

    [Fact]
    public void Default_Category_Is_Empty_String()
    {
        var violation = new Violation();

        Assert.Equal(string.Empty, violation.Category);
    }

    [Fact]
    public void Default_Message_Is_Empty_String()
    {
        var violation = new Violation();

        Assert.Equal(string.Empty, violation.Message);
    }
}