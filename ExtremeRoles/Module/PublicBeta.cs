using System.IO;
using BepInEx;
using BepInEx.Configuration;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class PublicBeta : NullableSingleton<PublicBeta>
{
	public string CurStateString { get; private set; } = string.Empty;

	public bool Enable
	{
		get
		{
			return this.getConfig().Value;
		}
		set
		{
			this.getConfig().Value = value;
		}
	}
	private ConfigEntry<bool>? enableConfig;

	private ConfigEntry<bool> getConfig()
	{
		if (this.enableConfig == null)
		{
			this.enableConfig = getConfig(ExtremeRolesPlugin.Instance.Config);
		}
		return this.enableConfig;
	}

	private static ConfigEntry<bool> getConfig(ConfigFile config)
		=> config.Bind<bool>(ClientOption.Selection, "PublicBetaMode", false);
}
