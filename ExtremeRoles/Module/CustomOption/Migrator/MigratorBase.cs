using BepInEx.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
		string[] allString = File.ReadAllLines(file.ConfigFilePath);
		string[] newValue = new string[allString.Length];
		var targets = ChangeOption;

		foreach (var (index, value) in allString.Select((value, index) => (index, value)))
		{
			string newLine = value;
			foreach (var (targetKey, replace) in targets)
			{
				if (value.StartsWith(targetKey))
				{
					ExtremeRolesPlugin.Logger.LogInfo($"Migrator: Update optionkey [{targetKey}] to [{replace}]");
					newLine = value.Replace(targetKey, replace);
					break;
				}
			}

			newValue[index] = newLine;
		}
		ExtremeRolesPlugin.Logger.LogInfo($"Save new config file....");
		File.WriteAllLines(file.ConfigFilePath, newValue);
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
