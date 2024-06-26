using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption.Migrator;

public static class MigratorManager
{
	public const int Version = 11;

	private const string category = "Compat";
	private const string key = "ConfigVersion";

	private static IReadOnlyList<MigratorBase> allMigrator => new List<MigratorBase>()
	{
		new V10toV11(),
	};

	private static ConfigDefinition def => new ConfigDefinition(category, key);

	public static bool IsMigrate(in ConfigFile config, out int version)
	{
		var def = MigratorManager.def;
		version = config.Bind(def, 0).Value;

		bool result = IsMigrate(version);

		// いらないので消す
		config.Remove(def);

		return result;
	}

	public static bool IsMigrate(in Version version) => IsMigrate(version.Major);
	public static bool IsMigrate(in int majorVersion)
		=> majorVersion < Version;

	public static void MigrateConfig(in ConfigFile config, int startVersion)
	{
		ExtremeRolesPlugin.Logger.LogInfo($"Migrating Config....");
		var def = MigratorManager.def;
		var entry = config.Bind(def, 0);

		foreach (MigratorBase migrator in allMigrator)
		{
			int checkVersion = migrator.TargetVersion;
			if (startVersion < checkVersion)
			{
				ExtremeRolesPlugin.Logger.LogInfo($"---- Start Migrating {startVersion} to {checkVersion} ----");
				migrator.MigrateConfig(config);
				startVersion = checkVersion;

				ExtremeRolesPlugin.Logger.LogInfo($"reloading config file....");
				config.Reload();

				entry.Value = startVersion;
				ExtremeRolesPlugin.Logger.LogInfo($"---- End Migrating ----");
			}
		}
		config.Remove(def);
		ExtremeRolesPlugin.Logger.LogInfo($"Migrating Complete");
	}

	public static void MigrateExportedOption(in Dictionary<string, int> importedOption, int startVersion)
	{
		ExtremeRolesPlugin.Logger.LogInfo($"Migrating ExportedOption....");
		foreach (MigratorBase migrator in allMigrator)
		{
			int checkVersion = migrator.TargetVersion;
			if (startVersion < checkVersion)
			{
				ExtremeRolesPlugin.Logger.LogInfo($"---- Start Migrating {startVersion} to {checkVersion} ----");
				migrator.MigrateExportedOption(importedOption);
				startVersion = checkVersion;
				ExtremeRolesPlugin.Logger.LogInfo($"---- End Migrating ----");
			}
		}

		ExtremeRolesPlugin.Logger.LogInfo($"Migrating Complete");
	}
}
