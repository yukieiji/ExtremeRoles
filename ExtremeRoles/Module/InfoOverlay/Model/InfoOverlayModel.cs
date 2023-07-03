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
								GhostRole,
								GlobalSetting
				}

				public bool IsDuty { get; set; }
				public Type CurShow { get; set; }

				public SortedDictionary<Type, IInfoOverlayPanelModel> PanelModel { get; set; }
}
