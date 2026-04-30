using ArchAudit.Cli.Models;
using Xunit;

namespace ArchAudit.Tests;

public sealed class ConfigTests
{
    [Fact]
    public void Default_Config_Has_Layer_Rules()
    {
        var config = ArchAuditConfig.CreateDefault();

        Assert.NotNull(config.Layer);
        Assert.NotEmpty(config.Layer!.ForbiddenRefs);
    }

    [Fact]
    public void Default_Config_Has_Circular_Deps_Enabled()
    {
        var config = ArchAuditConfig.CreateDefault();

        Assert.NotNull(config.CircularDeps);
        Assert.True(config.CircularDeps!.Enabled);
    }

    [Fact]
    public void Roundtrip_Yaml()
    {
        var original = ArchAuditConfig.CreateDefault();
        var yaml = original.ToYaml();

        Assert.NotEmpty(yaml);
        Assert.Contains("forbidden_refs", yaml);

        var deserialized = ArchAuditConfig.FromYaml(yaml);
        Assert.NotNull(deserialized.Layer);
    }

    [Fact]
    public void ForbiddenRef_Creates_Matches()
    {
        var config = ArchAuditConfig.CreateDefault();
        var refs = config.Layer!.ForbiddenRefs;

        Assert.Contains(refs, r => r.From == "*.UI" && r.To == "*.Data");
        Assert.Contains(refs, r => r.From == "*.UI" && r.To == "*.Infrastructure");
        Assert.Contains(refs, r => r.From == "*.Web" && r.To == "*.Data");
        Assert.Contains(refs, r => r.From == "*.Web" && r.To == "*.Infrastructure");
    }
}
