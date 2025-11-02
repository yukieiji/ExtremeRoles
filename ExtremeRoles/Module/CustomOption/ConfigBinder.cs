using BepInEx.Configuration;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.Module.CustomOption;

public sealed class ConfigBinder
{
	public int DefaultValue { get; }
	public int Value
	{
		get => config.Value;
		set
		{
			config.Value = value;
		}
	}

	private ConfigEntry<int> config;

	public ConfigBinder(string name, int @default)
	{
		DefaultValue = @default;

		config = GetOrCreateConfig(name);
	}

	public void Rebind()
	{
		string key = config.Definition.Key;
		config = GetOrCreateConfig(key);
	}
	private ConfigEntry<int> GetOrCreateConfig(string key)
		=> ExtremeRolesPlugin.Instance.Config.Bind(
			OldOptionManager.Instance.ConfigPreset, key, DefaultValue);
}
