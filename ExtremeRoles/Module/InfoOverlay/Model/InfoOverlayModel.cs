using System;
using System.Collections.Generic;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.InfoOverlay.Model;

public sealed class InfoOverlayModel
{
	public enum Type
	{
		MyRole,
		MyGhostRole,
		AllRole,
		AllGhostRole,
		GlobalSetting
	}

	public bool IsDuty { get; set; }
	public Type CurShow { get; set; }

	public SortedDictionary<Type, IInfoOverlayPanelModel> PanelModel { get; set; }

	public InfoOverlayModel()
	{
		this.PanelModel = new SortedDictionary<Type, IInfoOverlayPanelModel>();
		this.IsDuty = false;
		this.CurShow = Type.MyRole;
	}
}
