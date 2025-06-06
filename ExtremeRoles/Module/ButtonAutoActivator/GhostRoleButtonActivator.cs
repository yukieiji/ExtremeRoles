﻿using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ButtonAutoActivator
{
	public sealed class GhostRoleButtonActivator : IButtonAutoActivator
	{
		public bool IsActive()
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;

			return
				(
					localPlayer.IsKillTimerEnabled ||
					localPlayer.ForceKillTimerContinue ||
					HudManager.Instance.UseButton.isActiveAndEnabled
				) &&
				localPlayer.Data != null &&
				MeetingHud.Instance == null &&
				ExileController.Instance == null &&
				localPlayer.Data.IsDead;
		}
	}
}
