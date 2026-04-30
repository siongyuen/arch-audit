# arch-audit

[![CI](https://github.com/siongyuen/arch-audit/actions/workflows/ci.yml/badge.svg)](https://github.com/siongyuen/arch-audit/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/arch-audit.svg)](https://www.nuget.org/packages/arch-audit/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Architecture governance for .NET solutions** — catch forbidden cross-layer references, circular dependencies, naming convention breaks, and excessive coupling before they hit production.

```bash
dotnet tool install -g arch-audit
cd your-solution
arch-audit audit
```

## Why?

Every .NET team eventually discovers their architecture has drifted. A UI project starts referencing the data layer directly. Someone adds a sneaky circular dependency. A new project lands in the root directory instead of `src/`.

Most teams either ignore this until it hurts, or build a custom script. **arch-audit** gives you a reusable, configurable tool that runs locally or in CI, right now.

## Quick Start

```bash
# Install as a .NET global tool
dotnet tool install -g arch-audit

# Run an audit on your solution
cd MySolution/
arch-audit audit

# Generate a default configuration to customise
arch-audit init

# Run with JSON output for CI integration
arch-audit audit --format json --output report.json

# Strict mode: treat warnings as errors
arch-audit audit --strict
```

## Example Output

```markdown
# Architecture Audit Report

## Summary
| Severity | Count |
|----------|-------|
| 🔴 Error | 2     |
| 🟡 Warning | 1   |
| **Total** | **3** |

## Layer
- 🔴 **[Error]** Forbidden reference: 'MyApp.UI' references 'MyApp.Data'
  - *Project:* `MyApp.UI`
  - *Target:* `MyApp.Data`

## CircularDependency
- 🔴 **[Error]** Circular dependency detected: ProjectA → ProjectB → ProjectC → ProjectA

## Coupling
- 🟡 **[Warning]** Project 'GodProject' has 6 direct dependencies (max allowed: 5).
```

## Default Rules

| Rule | Description | Severity |
|------|-------------|----------|
| Layer | `*.UI` / `*.Web` must not reference `*.Data` / `*.Infrastructure` | Error |
| Circular | No circular dependencies between projects | Error |
| Naming | All projects should be under a `src/` directory | Warning |
| Coupling | Max 5 direct dependencies per project | Warning |

Override these by creating a `.archaudit.yml` file (use `arch-audit init` to generate one).

## Configuration

```yaml
rules:
  layer:
    forbidden_refs:
      - from: "*.UI"
        to: "*.Data"
      - from: "*.UI"
        to: "*.Infrastructure"
  circular_deps:
    enabled: true
  naming:
    src_directory_only: true
  coupling:
    max_direct_refs: 5
```

## CI Integration (GitHub Actions)

```yaml
- name: Run architecture audit
  run: |
    dotnet tool install -g arch-audit
    arch-audit audit --format json --output arch-audit-report.json

- name: Fail on violations
  run: |
    if grep -q '"passed": false' arch-audit-report.json; then
      echo "Architecture violations found!"
      exit 2
    fi
```

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Clean — no violations |
| 1 | Warnings only |
| 2 | Errors (or warnings in `--strict` mode) |

## Contributing

PRs welcome. Please ensure tests pass:

```bash
dotnet test
```

## License

MIT
