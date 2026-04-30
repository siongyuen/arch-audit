namespace ArchAudit.Cli.Models;

/// <summary>
/// Represents an architectural violation found during the audit.
/// </summary>
public sealed class Violation
{
    /// <summary>
    /// Gets or sets the severity level: Info, Warning, or Error.
    /// </summary>
    public ViolationSeverity Severity { get; set; } = ViolationSeverity.Error;

    /// <summary>
    /// Gets or sets the category of the violation (e.g., "Layer", "CircularDependency", "Naming", "Coupling").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the violation.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project this violation originated from.
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the target project or dependency involved.
    /// </summary>
    public string? Target { get; set; }

    /// <inheritdoc />
    public override string ToString() =>
        $"[{Severity}] {Category}: {Message}";
}

/// <summary>
/// Severity level for a violation.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>Informational — no action required.</summary>
    Info,

    /// <summary>Warning — may indicate a concern.</summary>
    Warning,

    /// <summary>Error — definite violation of architecture rules.</summary>
    Error
}
