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

	public static bool IsMigrate(in ConfigFile config)
	{
		var def = MigratorManager.def;
		int version = config.Bind(def, 10).Value;

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
		foreach (MigratorBase migrator in allMigrator)
		{
			int checkVersion = migrator.TargetVersion;
			if (checkVersion < startVersion)
			{
				migrator.MigrateConfig(config);
				startVersion = checkVersion;
			}
		}
	}

	public static void MigrateExportedOption(in Dictionary<string, int> importedOption, int startVersion)
	{
		foreach (MigratorBase migrator in allMigrator)
		{
			int checkVersion = migrator.TargetVersion;
			if (startVersion < checkVersion)
			{
				migrator.MigrateExportedOption(importedOption);
				startVersion = checkVersion;
			}
		}
	}
}
