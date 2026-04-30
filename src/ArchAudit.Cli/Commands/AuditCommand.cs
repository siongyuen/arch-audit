using System.CommandLine;
using System.CommandLine.Invocation;
using ArchAudit.Cli.Services;
using ArchAudit.Cli.Models;

namespace ArchAudit.Cli.Commands;

/// <summary>
/// The 'audit' command: scans a .NET solution and reports architecture violations.
/// </summary>
public sealed class AuditCommand : Command
{
    public AuditCommand()
        : base("audit", "Scan a .NET solution for architectural violations")
    {
        var pathArg = new Argument<string>(
            name: "path",
            description: "Path to the .sln file or directory to scan",
            getDefaultValue: () => ".");

        var formatOption = new Option<string>(
            name: "--format",
            description: "Output format (markdown or json)",
            getDefaultValue: () => "markdown");

        var strictOption = new Option<bool>(
            name: "--strict",
            description: "Treat warnings as errors",
            getDefaultValue: () => false);

        var outputOption = new Option<string>(
            name: "--output",
            description: "Write report to file instead of stdout",
            getDefaultValue: () => string.Empty);

        AddArgument(pathArg);
        AddOption(formatOption);
        AddOption(strictOption);
        AddOption(outputOption);

        this.SetHandler((string path, string format, bool strict, string output) =>
        {
            Run(path, format, strict, output);
        }, pathArg, formatOption, strictOption, outputOption);
    }

    private static void Run(string path, string format, bool strict, string outputFile)
    {
        // Load config
        var configLoader = new ConfigLoader();
        var config = configLoader.Load(path);

        // Scan
        var scanner = new SolutionScanner();
        Console.Error.WriteLine($"Scanning projects in '{path}'...");
        var projects = scanner.Scan(path);

        if (projects.Count == 0)
        {
            Console.Error.WriteLine("No .csproj files found. Nothing to audit.");
            return;
        }

        Console.Error.WriteLine($"Found {projects.Count} project(s). Analysing...");

        // Analyse
        var analyzer = new DependencyGraphAnalyzer(config);
        var violations = analyzer.Analyse(projects);

        // Generate report
        var options = new AuditOptions
        {
            Format = format,
            Strict = strict,
            Path = path
        };

        var reportGen = new ReportGenerator();
        var report = reportGen.Generate(violations, options);

        // Output
        if (!string.IsNullOrEmpty(outputFile))
        {
            File.WriteAllText(outputFile, report.Content);
            Console.Error.WriteLine($"Report written to '{outputFile}'");
        }
        else
        {
            Console.Out.WriteLine(report.Content);
        }

        // Determine exit code
        var hasErrors = violations.Any(v =>
            v.Severity == ViolationSeverity.Error ||
            (strict && v.Severity == ViolationSeverity.Warning));

        if (hasErrors)
        {
            Console.Error.WriteLine($"Audit completed with violations. Exit code: 2");
            Environment.Exit(2);
        }
        else if (violations.Any(v => v.Severity == ViolationSeverity.Warning))
        {
            Console.Error.WriteLine($"Audit completed with warnings. Exit code: 1");
            Environment.Exit(1);
        }
        else
        {
            Console.Error.WriteLine("Audit completed clean. Exit code: 0");
            Environment.Exit(0);
        }
    }
}
