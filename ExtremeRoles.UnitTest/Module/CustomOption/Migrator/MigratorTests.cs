using ExtremeRoles.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using ExtremeRoles.Module.CustomOption.Migrator;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Migrator;

public class MigratorTests : SerialTestBase, IClassFixture<GameOptionsManagerMock>, IClassFixture<CompatModManagerMock>
{
    public MigratorTests(SerialFixture fixture, GameOptionsManagerMock gameOptionsManagerMock, CompatModManagerMock compatModManagerMock)
        : base(fixture, gameOptionsManagerMock, compatModManagerMock, new LoggerMock())
    {
    }

    private sealed class TestMigrator : MigratorBase
    {
        private readonly int targetVersion;
        private readonly Dictionary<string, string> changeOption;

        public override int TargetVersion => targetVersion;

        protected override IReadOnlyDictionary<string, string> ChangeOption => changeOption;

        public TestMigrator(int targetVersion, Dictionary<string, string> changeOption)
        {
            this.targetVersion = targetVersion;
            this.changeOption = changeOption;
        }
    }

    [Fact]
    public void IsMigrate_MajorVersionLessThanCurrent_ReturnsTrue()
    {
        Assert.True(MigratorManager.IsMigrate(10));
        Assert.True(MigratorManager.IsMigrate(0));
    }

    [Fact]
    public void IsMigrate_MajorVersionEqualsOrGreaterThanCurrent_ReturnsFalse()
    {
        Assert.False(MigratorManager.IsMigrate(MigratorManager.Version));
        Assert.False(MigratorManager.IsMigrate(12));
    }

    [Fact]
    public void IsMigrate_VersionObject_ChecksMajorVersionCorrectly()
    {
        var oldVer = new Version(10, 5, 0);
        var currentVer = new Version(MigratorManager.Version, 0, 0);
        var newVer = new Version(12, 0, 0);

        Assert.True(MigratorManager.IsMigrate(oldVer));
        Assert.False(MigratorManager.IsMigrate(currentVer));
        Assert.False(MigratorManager.IsMigrate(newVer));
    }

    [Fact]
    public void IsMigrate_ConfigFile_ReadsVersionAndDeterminesMigration()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid():N}.cfg");
        try
        {
            File.WriteAllText(tempFile, "[Compat]\nConfigVersion = 10\n");
            var config = new ConfigFile(tempFile, true);

            bool result = MigratorManager.IsMigrate(config, out int version);

            Assert.True(result);
            Assert.Equal(10, version);
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
    public void MigrateConfig_UpdatesMatchingLinesInConfigFile()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid():N}.cfg");
        try
        {
            File.WriteAllText(tempFile, "[Section]\nOldKey = 100\nUnchangedKey = 200\n");
            var config = new ConfigFile(tempFile, true);

            var migrator = new TestMigrator(11, new Dictionary<string, string>
            {
                { "OldKey", "NewKey" }
            });

            migrator.MigrateConfig(config);

            string[] lines = File.ReadAllLines(tempFile);
            Assert.Contains("NewKey = 100", lines);
            Assert.Contains("UnchangedKey = 200", lines);
            Assert.DoesNotContain("OldKey = 100", lines);
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
    public void MigrateExportedOption_UpdatesDictionaryKeys()
    {
        var migrator = new TestMigrator(11, new Dictionary<string, string>
        {
            { "OldOptionA", "NewOptionA" },
            { "OldOptionB", "NewOptionB" }
        });

        var options = new Dictionary<string, int>
        {
            { "OldOptionA", 1 },
            { "UnchangedOption", 5 }
        };

        migrator.MigrateExportedOption(options);

        Assert.True(options.ContainsKey("NewOptionA"));
        Assert.Equal(1, options["NewOptionA"]);
        Assert.Equal(5, options["UnchangedOption"]);
    }

    [Fact]
    public void V10toV11_TargetVersion_Is11()
    {
        var v10tov11 = new V10toV11();
        Assert.Equal(11, v10tov11.TargetVersion);
    }

    [Fact]
    public void V10toV11_MigrateExportedOption_MigratesKnownKeys()
    {
        var v10tov11 = new V10toV11();
        var options = new Dictionary<string, int>
        {
            { "UseRaiseHand", 1 },
            { "MinCrewmateRoles", 2 },
            { "UnchangedKey", 99 }
        };

        v10tov11.MigrateExportedOption(options);

        Assert.True(options.ContainsKey("MeetingOptionUseRaiseHand"));
        Assert.Equal(1, options["MeetingOptionUseRaiseHand"]);

        Assert.True(options.ContainsKey("RoleSpawnCategoryMinCrewmate"));
        Assert.Equal(2, options["RoleSpawnCategoryMinCrewmate"]);

        Assert.Equal(99, options["UnchangedKey"]);
    }

    [Fact]
    public void MigratorManager_MigrateConfig_ExecutesAllApplicableMigratorsAndUpdateConfigVersion()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid():N}.cfg");
        try
        {
            File.WriteAllText(tempFile, "[Compat]\nConfigVersion = 10\n[Section]\nUseRaiseHand = 1\n");
            var config = new ConfigFile(tempFile, true);

            MigratorManager.MigrateConfig(config, 10);

            var entry = config.Bind("Compat", "ConfigVersion", 0);
            Assert.Equal(11, entry.Value);

            string[] lines = File.ReadAllLines(tempFile);
            Assert.Contains("MeetingOptionUseRaiseHand = 1", lines);
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
    public void MigratorManager_MigrateExportedOption_ExecutesAllApplicableMigrators()
    {
        var options = new Dictionary<string, int>
        {
            { "UseRaiseHand", 1 }
        };

        MigratorManager.MigrateExportedOption(options, 10);

        Assert.True(options.ContainsKey("MeetingOptionUseRaiseHand"));
        Assert.Equal(1, options["MeetingOptionUseRaiseHand"]);
    }

    [Fact]
    public void MigratorBase_Dispose_DoesNotThrow()
    {
        using var migrator = new V10toV11();
        // Verify Dispose can be called without error
    }
}