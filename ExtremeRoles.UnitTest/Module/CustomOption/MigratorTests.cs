using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using ExtremeRoles.Module.CustomOption.Migrator;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption;

public class TestMigrator : MigratorBase
{
    public override int TargetVersion => 2;

    protected override IReadOnlyDictionary<string, string> ChangeOption => new Dictionary<string, string>
    {
        { "OldKey1", "NewKey1" },
        { "OldKey2", "NewKey2" }
    };
}

[Collection("UnityMock")]
public class MigratorTests
{
    public MigratorTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
    }

    [Fact]
    public void MigratorBase_MigrateConfig_ShouldReplaceOldKeysInFile()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".cfg");
        try
        {
            File.WriteAllLines(tempFile, new[]
            {
                "OldKey1 = 100",
                "UnchangedKey = 50",
                "OldKey2 = 200"
            });

            var config = new ConfigFile(tempFile, saveOnInit: false);

            using var migrator = new TestMigrator();
            migrator.MigrateConfig(config);

            string[] lines = File.ReadAllLines(tempFile);
            Assert.Contains("NewKey1 = 100", lines);
            Assert.Contains("UnchangedKey = 50", lines);
            Assert.Contains("NewKey2 = 200", lines);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void MigratorBase_MigrateExportedOption_ShouldReplaceDictionaryKeys()
    {
        var dict = new Dictionary<string, int>
        {
            { "OldKey1", 10 },
            { "Unchanged", 20 }
        };

        using var migrator = new TestMigrator();
        migrator.MigrateExportedOption(dict);

        Assert.True(dict.ContainsKey("NewKey1"));
        Assert.Equal(10, dict["NewKey1"]);
        Assert.True(dict.ContainsKey("Unchanged"));
        Assert.True(dict.ContainsKey("OldKey1"));
    }

    [Fact]
    public void MigratorManager_IsMigrate_ShouldCheckVersionCorrectly()
    {
        Assert.True(MigratorManager.IsMigrate(10));
        Assert.False(MigratorManager.IsMigrate(11));
        Assert.False(MigratorManager.IsMigrate(12));

        var ver10 = new Version(10, 0);
        var ver11 = new Version(11, 0);
        Assert.True(MigratorManager.IsMigrate(ver10));
        Assert.False(MigratorManager.IsMigrate(ver11));
    }

    [Fact]
    public void MigratorManager_MigrateExportedOption_ShouldMigrateV10ToV11Key()
    {
        var dict = new Dictionary<string, int>
        {
            { "UseRaiseHand", 1 },
            { "NumMeating", 5 }
        };

        MigratorManager.MigrateExportedOption(dict, startVersion: 10);

        Assert.True(dict.ContainsKey("UseRaiseHand"));
        Assert.True(dict.ContainsKey("NumMeating"));
        Assert.True(dict.Count > 2);
    }
}
