using BepInEx.Configuration;

namespace ExtremeRoles.Module.NewOption;

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
			NewOptionManager.Instance.ConfigPreset, key, DefaultValue);
}
