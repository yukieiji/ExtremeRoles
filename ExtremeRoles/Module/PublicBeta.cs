using ExtremeRoles.Helper;

using BepInEx.Configuration;

using ExtremeRoles.Beta;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class PublicBeta : NullableSingleton<PublicBeta>
{
	public string CurStateString { get; private set; } = string.Empty;

	public enum Mode
	{
		Enable,
		Disable,
		EnableReady,
		DisableReady
	}

	public bool Enable
	{
		get
		{
			return this.getConfig().Value;
		}
	}

	private ConfigEntry<bool>? enableConfig;
	private Mode mode;

	public PublicBeta()
	{
		this.mode = this.Enable ? Mode.Enable : Mode.Disable;
		ExtremeRolesPlugin.Logger.LogInfo($"PublicBeta Mode : {this.Enable}");
		this.updateStatusString();
	}

	public void SwitchMode()
	{
		this.mode = this.Enable ? Mode.DisableReady : Mode.EnableReady;
		this.getConfig().Value = !this.Enable;
		ExtremeRolesPlugin.Logger.LogInfo($"Switch Mode: {this.mode}");
		updateStatusString();
	}

	private void updateStatusString()
	{
		this.CurStateString = this.mode switch
		{
			Mode.Enable => $"パブリックベータ - v{BetaContentManager.Version}",
			Mode.DisableReady => "再起動後にパブリックベータモードが無効になります",
			Mode.EnableReady => "再起動後にパブリックベータモードが有効になります",
			_ => string.Empty
		};
		StatusTextShower.Instance.RebuildVersionShower();
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
