using BepInEx.Configuration;

using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption.Migrator;

public abstract class MigratorBase : IDisposable
{
	public abstract int TargetVersion { get; }

	protected abstract IReadOnlyDictionary<string, string> ChangeOption { get; }

	protected static string S<T>(T key) => key.ToString();

	public void Dispose()
	{ }

	public void MigrateConfig(in ConfigFile file)
	{

	}

	public void MigrateExportedOption(in Dictionary<string, int> importedOption)
	{
		foreach (var (oldKey, newKey) in ChangeOption)
		{
			if (importedOption.TryGetValue(oldKey, out int value))
			{
				ExtremeRolesPlugin.Logger.LogInfo($"Migrator: Update optionkey [{oldKey}] to [{newKey}]");
				importedOption[newKey] = value;
			}
		}
	}
}
