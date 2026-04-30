using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArchAudit.Cli.Models;

/// <summary>
/// Configuration model for arch-audit rules, deserialised from .archaudit.yml.
/// </summary>
public sealed class ArchAuditConfig
{
    /// <summary>Gets or sets the layer rules.</summary>
    [YamlMember(Alias = "layer")]
    public LayerConfig? Layer { get; set; }

    /// <summary>Gets or sets the circular dependency rules.</summary>
    [YamlMember(Alias = "circular_deps")]
    public CircularDepsConfig? CircularDeps { get; set; }

    /// <summary>Gets or sets the naming rules.</summary>
    [YamlMember(Alias = "naming")]
    public NamingConfig? Naming { get; set; }

    /// <summary>Gets or sets the coupling rules.</summary>
    [YamlMember(Alias = "coupling")]
    public CouplingConfig? Coupling { get; set; }

    /// <summary>
    /// Creates a default configuration with sensible architecture rules.
    /// </summary>
    public static ArchAuditConfig CreateDefault()
    {
        return new ArchAuditConfig
        {
            Layer = new LayerConfig
            {
                ForbiddenRefs =
                [
                    new ForbiddenRef { From = "*.UI", To = "*.Data" },
                    new ForbiddenRef { From = "*.UI", To = "*.Infrastructure" },
                    new ForbiddenRef { From = "*.Web", To = "*.Data" },
                    new ForbiddenRef { From = "*.Web", To = "*.Infrastructure" },
                ]
            },
            CircularDeps = new CircularDepsConfig { Enabled = true },
            Naming = new NamingConfig { SrcDirectoryOnly = true },
            Coupling = new CouplingConfig { MaxDirectRefs = 5 }
        };
    }

    /// <summary>
    /// Serialises this config to YAML.
    /// </summary>
    public string ToYaml()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return serializer.Serialize(this);
    }

    /// <summary>
    /// Deserialises a configuration from a YAML string.
    /// </summary>
    public static ArchAuditConfig FromYaml(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<ArchAuditConfig>(yaml);
    }
}

public sealed class LayerConfig
{
    [YamlMember(Alias = "forbidden_refs")]
    public List<ForbiddenRef> ForbiddenRefs { get; set; } = [];
}

public sealed class ForbiddenRef
{
    [YamlMember(Alias = "from")]
    public string From { get; set; } = string.Empty;

    [YamlMember(Alias = "to")]
    public string To { get; set; } = string.Empty;
}

public sealed class CircularDepsConfig
{
    [YamlMember(Alias = "enabled")]
    public bool Enabled { get; set; } = true;
}

public sealed class NamingConfig
{
    [YamlMember(Alias = "src_directory_only")]
    public bool SrcDirectoryOnly { get; set; }
}

public sealed class CouplingConfig
{
    [YamlMember(Alias = "max_direct_refs")]
    public int MaxDirectRefs { get; set; } = 5;
}
