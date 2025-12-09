using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.InfoOverlay.Model;

public sealed class InfoOverlayModel
{
	public enum Type : byte
	{
		YourRolePanel,
		YourGhostRolePanel,
		AllRolePanel,
		AllGhostRolePanel,
		GlobalSettingPanel,
		Liberal
	}

	public bool IsDuty { get; set; }
	public Type CurShow { get; set; }

	public SortedDictionary<Type, IInfoOverlayPanelModel> PanelModel { get; set; }

	public bool IsShowActiveOnly
	{
		get => configShowActiveOnly.Value;
		set => configShowActiveOnly.Value = value;
	}

	private readonly ConfigEntry<bool> configShowActiveOnly;

	public InfoOverlayModel()
	{
		this.PanelModel = new SortedDictionary<Type, IInfoOverlayPanelModel>();
		this.IsDuty = false;
		this.CurShow = Type.YourRolePanel;

		this.configShowActiveOnly = ExtremeRolesPlugin.Instance.Config.Bind("InfoOverlay", "ShowActiveOnly", false);
	}
}
