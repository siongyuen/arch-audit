using ArchAudit.Cli.Models;

namespace ArchAudit.Cli.Services;

/// <summary>
/// Scans a directory for .csproj files and parses project references.
/// </summary>
public sealed class SolutionScanner
{
    /// <summary>
    /// Discovers all .csproj files in the specified path and parses them into ProjectNodes.
    /// </summary>
    public List<ProjectNode> Scan(string path)
    {
        // Resolve the search path
        string searchPath;
        if (File.Exists(path) && path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            searchPath = Path.GetDirectoryName(path) ?? ".";
        }
        else if (Directory.Exists(path))
        {
            searchPath = path;
        }
        else
        {
            searchPath = ".";
        }

        var csprojFiles = Directory.GetFiles(searchPath, "*.csproj", SearchOption.AllDirectories);
        var projects = new List<ProjectNode>();

        foreach (var file in csprojFiles)
        {
            var project = ParseProject(file);
            if (project != null)
            {
                projects.Add(project);
            }
        }

        return projects;
    }

    private static ProjectNode? ParseProject(string csprojPath)
    {
        try
        {
            var content = File.ReadAllText(csprojPath);
            var name = Path.GetFileNameWithoutExtension(csprojPath);
            var dir = Path.GetDirectoryName(csprojPath) ?? ".";

            var project = new ProjectNode
            {
                Name = name,
                FilePath = csprojPath,
                Directory = dir,
                References = ParseProjectReferences(content),
                PackageReferences = ParsePackageReferences(content)
            };

            return project;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not parse {csprojPath}: {ex.Message}");
            return null;
        }
    }

    private static List<string> ParseProjectReferences(string csprojContent)
    {
        var refs = new List<string>();
        // Simple regex-based parsing for <ProjectReference Include="..\Path\To\Project.csproj" />
        var matches = System.Text.RegularExpressions.Regex.Matches(
            csprojContent,
            @"<ProjectReference\s+Include\s*=\s*""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var includePath = match.Groups[1].Value;
            var projectName = Path.GetFileNameWithoutExtension(includePath);
            if (!string.IsNullOrEmpty(projectName))
            {
                refs.Add(projectName);
            }
        }

        return refs;
    }

    private static List<string> ParsePackageReferences(string csprojContent)
    {
        var refs = new List<string>();
        var matches = System.Text.RegularExpressions.Regex.Matches(
            csprojContent,
            @"<PackageReference\s+Include\s*=\s*""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            refs.Add(match.Groups[1].Value);
        }

        return refs;
    }
}
