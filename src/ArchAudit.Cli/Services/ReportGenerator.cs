using System.Text;
using ArchAudit.Cli.Models;

namespace ArchAudit.Cli.Services;

/// <summary>
/// Generates audit reports in markdown or JSON format.
/// </summary>
public sealed class ReportGenerator
{
    /// <summary>
    /// Generates a report from the list of violations.
    /// </summary>
    public ReportResult Generate(List<Violation> violations, AuditOptions options)
    {
        return options.Format switch
        {
            "json" => GenerateJson(violations),
            _ => GenerateMarkdown(violations)
        };
    }

    private static ReportResult GenerateMarkdown(List<Violation> violations)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Architecture Audit Report");
        sb.AppendLine();
        sb.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine();

        var errors = violations.Count(v => v.Severity == ViolationSeverity.Error);
        var warnings = violations.Count(v => v.Severity == ViolationSeverity.Warning);
        var infos = violations.Count(v => v.Severity == ViolationSeverity.Info);

        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"| Severity | Count |");
        sb.AppendLine($"|----------|-------|");
        sb.AppendLine($"| 🔴 Error | {errors} |");
        sb.AppendLine($"| 🟡 Warning | {warnings} |");
        sb.AppendLine($"| 🔵 Info | {infos} |");
        sb.AppendLine($"| **Total** | **{violations.Count}** |");
        sb.AppendLine();

        if (violations.Count == 0)
        {
            sb.AppendLine("✅ **No violations found. Your architecture is clean!**");
            sb.AppendLine();
            return new ReportResult(sb.ToString(), violations.Count == 0, "markdown");
        }

        var categories = violations.GroupBy(v => v.Category);
        foreach (var category in categories)
        {
            sb.AppendLine($"## {category.Key}");
            sb.AppendLine();

            foreach (var violation in category)
            {
                var icon = violation.Severity switch
                {
                    ViolationSeverity.Error => "🔴",
                    ViolationSeverity.Warning => "🟡",
                    ViolationSeverity.Info => "🔵",
                    _ => "⚪"
                };

                sb.AppendLine($"- {icon} **[{violation.Severity}]** {violation.Message}");
                if (!string.IsNullOrEmpty(violation.ProjectName))
                {
                    sb.AppendLine($"  - *Project:* `{violation.ProjectName}`");
                }
                if (!string.IsNullOrEmpty(violation.Target))
                {
                    sb.AppendLine($"  - *Target:* `{violation.Target}`");
                }
            }

            sb.AppendLine();
        }

        return new ReportResult(sb.ToString(), errors == 0, "markdown");
    }

    private static ReportResult GenerateJson(List<Violation> violations)
    {
        var errorCount = violations.Count(v => v.Severity == ViolationSeverity.Error);
        var warningCount = violations.Count(v => v.Severity == ViolationSeverity.Warning);
        var infoCount = violations.Count(v => v.Severity == ViolationSeverity.Info);

        var jsonViolations = violations.Select(v => new
        {
            severity = v.Severity.ToString().ToLowerInvariant(),
            category = v.Category,
            message = v.Message,
            project = v.ProjectName,
            target = v.Target
        });

        var report = new
        {
            generated_at = DateTime.UtcNow.ToString("o"),
            summary = new
            {
                total = violations.Count,
                errors = errorCount,
                warnings = warningCount,
                infos = infoCount,
                passed = errorCount == 0
            },
            violations = jsonViolations
        };

        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        return new ReportResult(json, errorCount == 0, "json");
    }
}

/// <summary>
/// Options for report generation.
/// </summary>
public sealed class AuditOptions
{
    /// <summary>Report format: "markdown" or "json".</summary>
    public string Format { get; set; } = "markdown";

    /// <summary>When true, warnings are treated as errors.</summary>
    public bool Strict { get; set; }

    /// <summary>Path to the solution or directory to scan.</summary>
    public string Path { get; set; } = ".";
}

/// <summary>
/// Result of report generation.
/// </summary>
public sealed class ReportResult
{
    public string Content { get; }
    public bool Passed { get; }
    public string Format { get; }

    public ReportResult(string content, bool passed, string format)
    {
        Content = content;
        Passed = passed;
        Format = format;
    }
}
