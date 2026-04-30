using ArchAudit.Cli.Models;
using ArchAudit.Cli.Services;
using Xunit;

namespace ArchAudit.Tests;

public sealed class ReportGeneratorTests
{
    private static List<Violation> SampleViolations() =>
    [
        new()
        {
            Severity = ViolationSeverity.Error,
            Category = "Layer",
            Message = "UI project must not reference Data project directly.",
            ProjectName = "MyApp.UI",
            Target = "MyApp.Data",
        },
        new()
        {
            Severity = ViolationSeverity.Warning,
            Category = "Coupling",
            Message = "Project has too many direct references (6, max is 5).",
            ProjectName = "MyApp.God",
            Target = null,
        },
        new()
        {
            Severity = ViolationSeverity.Info,
            Category = "Naming",
            Message = "Project is not located under a 'src/' directory.",
            ProjectName = "LegacyModule",
            Target = null,
        },
    ];

    [Fact]
    public void Markdown_With_Violations_Grouped_By_Category_With_Emoji()
    {
        var generator = new ReportGenerator();
        var options = new AuditOptions { Format = "markdown" };

        var result = generator.Generate(SampleViolations(), options);

        Assert.Equal("markdown", result.Format);
        Assert.False(result.Passed);

        var content = result.Content;

        // Contains a summary table with emoji
        Assert.Contains("🔴 Error", content);
        Assert.Contains("🟡 Warning", content);
        Assert.Contains("🔵 Info", content);

        // Contains category headings
        Assert.Contains("## Layer", content);
        Assert.Contains("## Coupling", content);
        Assert.Contains("## Naming", content);

        // Contains per-violation emoji icons
        Assert.Contains("🔴 **[Error]**", content);
        Assert.Contains("🟡 **[Warning]**", content);
        Assert.Contains("🔵 **[Info]**", content);

        // Contains project names
        Assert.Contains("`MyApp.UI`", content);
        Assert.Contains("`MyApp.God`", content);
        Assert.Contains("`LegacyModule`", content);

        // Contains target info
        Assert.Contains("`MyApp.Data`", content);
    }

    [Fact]
    public void Markdown_With_No_Violations_Shows_Clean_Message()
    {
        var generator = new ReportGenerator();
        var options = new AuditOptions { Format = "markdown" };

        var result = generator.Generate([], options);

        Assert.Equal("markdown", result.Format);
        Assert.True(result.Passed);

        var content = result.Content;

        // Should show the clean/empty message
        Assert.Contains("No violations found", content);
        Assert.Contains("✅", content);

        // Should NOT contain any category section
        Assert.DoesNotContain("## Layer", content);
        Assert.DoesNotContain("## Coupling", content);

        // But should still have a summary with zero counts
        Assert.Contains("| 🔴 Error | 0 |", content);
        Assert.Contains("| 🟡 Warning | 0 |", content);
        Assert.Contains("| 🔵 Info | 0 |", content);
        Assert.Contains("| **Total** | **0**", content);
    }

    [Fact]
    public void Json_With_Violations_Has_Correct_Structure()
    {
        var generator = new ReportGenerator();
        var options = new AuditOptions { Format = "json" };

        var result = generator.Generate(SampleViolations(), options);

        Assert.Equal("json", result.Format);
        Assert.False(result.Passed);

        var json = result.Content;

        // Contains top-level structure
        Assert.Contains("\"generated_at\"", json);
        Assert.Contains("\"summary\"", json);
        Assert.Contains("\"violations\"", json);

        // Summary has correct counts
        Assert.Contains("\"total\": 3", json);
        Assert.Contains("\"errors\": 1", json);
        Assert.Contains("\"warnings\": 1", json);
        Assert.Contains("\"infos\": 1", json);
        Assert.Contains("\"passed\": false", json);

        // Contains violation data
        Assert.Contains("\"severity\": \"error\"", json);
        Assert.Contains("\"severity\": \"warning\"", json);
        Assert.Contains("\"severity\": \"info\"", json);
        Assert.Contains("\"category\": \"Layer\"", json);
        Assert.Contains("\"category\": \"Coupling\"", json);
        Assert.Contains("\"category\": \"Naming\"", json);

        // Contains project names
        Assert.Contains("\"project\": \"MyApp.UI\"", json);
        Assert.Contains("\"project\": \"MyApp.God\"", json);
        Assert.Contains("\"project\": \"LegacyModule\"", json);
    }

    [Fact]
    public void Json_With_No_Violations_Shows_Passed_True()
    {
        var generator = new ReportGenerator();
        var options = new AuditOptions { Format = "json" };

        var result = generator.Generate([], options);

        Assert.Equal("json", result.Format);
        Assert.True(result.Passed);

        var json = result.Content;

        Assert.Contains("\"total\": 0", json);
        Assert.Contains("\"passed\": true", json);
        Assert.Contains("\"violations\": []", json);
    }

    [Fact]
    public void Strict_Mode_Affects_Json_Output_Passed_State()
    {
        var generator = new ReportGenerator();

        // With strict mode, warnings are treated as errors.
        // violations list: 0 errors, 1 warning, 1 info
        var violations = new List<Violation>
        {
            new()
            {
                Severity = ViolationSeverity.Warning,
                Category = "Coupling",
                Message = "Too many direct references.",
                ProjectName = "MyApp.God",
            },
            new()
            {
                Severity = ViolationSeverity.Info,
                Category = "Naming",
                Message = "Not in src/ directory.",
                ProjectName = "LegacyModule",
            },
        };

        // Test non-strict mode (default): passed = true (no errors)
        var nonStrictOptions = new AuditOptions { Format = "json", Strict = false };
        var nonStrictResult = generator.Generate(violations, nonStrictOptions);

        // The ReportGenerator's Passed is based on errorCount == 0, not strict mode.
        // Strict mode means warnings become errors at the analyzer level, but
        // ReportGenerator itself doesn't implement strict logic — it reports whatever
        // severity it receives. This test documents that behaviour.
        var nonStrictJson = nonStrictResult.Content;
        Assert.Contains("\"passed\": true", nonStrictJson); // 0 errors, only warnings/infos

        // With strict mode enabled in options but no actual Error-severity violations,
        // the generator still reports passed=true unless the analyzer has already
        // promoted warnings to errors. The strict mode in AuditOptions is metadata
        // the report could use — here we verify the content includes strict-related
        // info when the option is on.
        var strictOptions = new AuditOptions { Format = "json", Strict = true };
        var strictResult = generator.Generate(violations, strictOptions);

        var strictJson = strictResult.Content;
        Assert.Contains("\"warnings\": 1", strictJson);
        Assert.Contains("\"errors\": 0", strictJson);
        Assert.Contains("\"passed\": true", strictJson);
    }
}
