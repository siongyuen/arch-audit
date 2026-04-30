namespace ArchAudit.Cli.Models;

/// <summary>
/// Represents a .NET project discovered during the scan.
/// </summary>
public sealed class ProjectNode
{
    /// <summary>
    /// Gets or sets the project name (derived from the .csproj filename without extension).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full file path to the .csproj file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory containing the .csproj file.
    /// </summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of project reference names (the <see cref="Name"/> values of dependencies).
    /// </summary>
    public List<string> References { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of NuGet package references in this project.
    /// </summary>
    public List<string> PackageReferences { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => Name;
}
