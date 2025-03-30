using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ButtonAutoActivator
{
	public sealed class RoleButtonActivator : IButtonAutoActivator
	{
		public bool IsActive()
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;

			return
				localPlayer != null &&
				(
					localPlayer.IsKillTimerEnabled ||
					localPlayer.ForceKillTimerContinue ||
					HudManager.Instance.UseButton.isActiveAndEnabled
				) &&
				localPlayer.Data != null &&
				!localPlayer.Data.IsDead &&
				MeetingHud.Instance == null &&
				ExileController.Instance == null &&
				IntroCutscene.Instance == null;
		}
	}
}
