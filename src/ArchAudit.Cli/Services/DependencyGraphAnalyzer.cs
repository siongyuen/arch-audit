using ArchAudit.Cli.Models;

namespace ArchAudit.Cli.Services;

/// <summary>
/// Analyses the project dependency graph against the configured rules and produces violations.
/// </summary>
public sealed class DependencyGraphAnalyzer
{
    private readonly ArchAuditConfig _config;

    public DependencyGraphAnalyzer(ArchAuditConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Runs all configured rules against the project list and returns any violations found.
    /// </summary>
    public List<Violation> Analyse(List<ProjectNode> projects)
    {
        var violations = new List<Violation>();

        violations.AddRange(CheckLayerViolations(projects));
        violations.AddRange(CheckCircularDependencies(projects));
        violations.AddRange(CheckNamingConventions(projects));
        violations.AddRange(CheckCoupling(projects));

        return violations;
    }

    private List<Violation> CheckLayerViolations(List<ProjectNode> projects)
    {
        var violations = new List<Violation>();
        var forbiddenRefs = _config.Layer?.ForbiddenRefs ?? [];

        foreach (var rule in forbiddenRefs)
        {
            var fromPattern = rule.From.Replace("*", ".*");
            var toPattern = rule.To.Replace("*", ".*");

            foreach (var project in projects)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(project.Name, fromPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    continue;

                foreach (var reference in project.References)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(reference, toPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        violations.Add(new Violation
                        {
                            Severity = ViolationSeverity.Error,
                            Category = "Layer",
                            Message = $"Forbidden reference: '{project.Name}' references '{reference}' which violates layer rules.",
                            ProjectName = project.Name,
                            Target = reference
                        });
                    }
                }
            }
        }

        return violations;
    }

    private List<Violation> CheckCircularDependencies(List<ProjectNode> projects)
    {
        var violations = new List<Violation>();
        if (_config.CircularDeps?.Enabled != true)
            return violations;

        var projectMap = projects.ToDictionary(p => p.Name, p => p);

        foreach (var project in projects)
        {
            var visited = new HashSet<string>();
            var path = new List<string>();
            DetectCycle(project.Name, projectMap, visited, path, violations);
        }

        return violations;
    }

    private void DetectCycle(
        string current,
        Dictionary<string, ProjectNode> projectMap,
        HashSet<string> visited,
        List<string> path,
        List<Violation> violations)
    {
        if (path.Contains(current))
        {
            // Found a cycle — report it once
            var cycleIndex = path.IndexOf(current);
            var cycle = path[cycleIndex..];
            cycle.Add(current); // close the loop

            var cycleStr = string.Join(" → ", cycle);
            var existing = violations.Any(v =>
                v.Category == "CircularDependency" &&
                v.Message.Contains(cycleStr));

            if (!existing)
            {
                violations.Add(new Violation
                {
                    Severity = ViolationSeverity.Error,
                    Category = "CircularDependency",
                    Message = $"Circular dependency detected: {cycleStr}",
                    ProjectName = current
                });
            }
            return;
        }

        if (visited.Contains(current))
            return;

        visited.Add(current);
        path.Add(current);

        if (projectMap.TryGetValue(current, out var project))
        {
            foreach (var dep in project.References)
            {
                DetectCycle(dep, projectMap, visited, path, violations);
            }
        }

        path.RemoveAt(path.Count - 1);
    }

    private List<Violation> CheckNamingConventions(List<ProjectNode> projects)
    {
        var violations = new List<Violation>();
        if (_config.Naming?.SrcDirectoryOnly != true)
            return violations;

        foreach (var project in projects)
        {
            var dir = project.Directory.Replace('\\', '/');
            if (!dir.Contains("/src/") && !dir.StartsWith("src/") && !dir.Equals("src", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new Violation
                {
                    Severity = ViolationSeverity.Warning,
                    Category = "Naming",
                    Message = $"Project '{project.Name}' is not under a 'src/' directory (found in '{project.Directory}').",
                    ProjectName = project.Name
                });
            }
        }

        return violations;
    }

    private List<Violation> CheckCoupling(List<ProjectNode> projects)
    {
        var violations = new List<Violation>();
        var maxRefs = _config.Coupling?.MaxDirectRefs ?? 5;

        foreach (var project in projects)
        {
            if (project.References.Count > maxRefs)
            {
                violations.Add(new Violation
                {
                    Severity = ViolationSeverity.Warning,
                    Category = "Coupling",
                    Message = $"Project '{project.Name}' has {project.References.Count} direct dependencies (max allowed: {maxRefs}). Consider refactoring.",
                    ProjectName = project.Name
                });
            }
        }

        return violations;
    }
}
