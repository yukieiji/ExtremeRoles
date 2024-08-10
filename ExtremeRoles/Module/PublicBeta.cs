using ExtremeRoles.Helper;

using BepInEx.Configuration;

using ExtremeRoles.Beta;



#nullable enable

namespace ExtremeRoles.Module;

public sealed class PublicBeta : NullableSingleton<PublicBeta>
{
	public string CurStateString
	{
		get
		{
			if (this.mode is Mode.Disable)
			{
				return string.Empty;
			}

			if (this.modeStr == string.Empty)
			{
				this.updateStatusString();
			}
			return this.modeStr;
		}
	}

	public enum Mode
	{
		Enable,
		Disable,
		EnableReady,
		DisableReady
	}

	public bool IsEnableWithMode => this.Enable && this.mode == Mode.Enable;

	public bool Enable
	{
		get
		{
			return this.getConfig().Value;
		}
	}

	private ConfigEntry<bool>? enableConfig;
	private Mode mode;

	private string modeStr = string.Empty;

	public PublicBeta()
	{
		this.mode = this.Enable ? Mode.Enable : Mode.Disable;
		ExtremeRolesPlugin.Logger.LogInfo($"PublicBeta Mode : {this.Enable}");
	}

	public void SwitchMode()
	{
		this.mode = this.Enable ? Mode.DisableReady : Mode.EnableReady;
		this.getConfig().Value = !this.Enable;
		ExtremeRolesPlugin.Logger.LogInfo($"Switch Mode: {this.mode}");

		updateStatusString();

		StatusTextShower.Instance.RebuildVersionShower();
	}

	private void updateStatusString()
	{
		this.modeStr = string.Empty;

		var (formatKey, value) = this.mode switch
		{
			Mode.Enable => ("PublicBetaStr", BetaContentManager.Version),
			Mode.DisableReady => ("PublicBetaEnableDisableStr", Tr.GetString("DisableKey")),
			Mode.EnableReady => ("PublicBetaEnableDisableStr", Tr.GetString("EnableKey")),
			_ => (string.Empty, string.Empty)
		};

		if (string.IsNullOrEmpty(formatKey)) { return; }

		this.modeStr = Tr.GetString(formatKey, value);
	}

	private ConfigEntry<bool> getConfig()
	{
		if (this.enableConfig == null)
		{
			this.enableConfig = getConfig(ExtremeRolesPlugin.Instance.Config);
		}
		return this.enableConfig;
	}

	private static ConfigEntry<bool> getConfig(ConfigFile config)
		=> config.Bind(ClientOption.Selection, "PublicBetaMode", false);
}
