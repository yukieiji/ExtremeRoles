using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.Ability.AutoActivator;

public sealed class RoleButtonActivator : IButtonAutoActivator
{
	public bool IsActive()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;

		return
			localPlayer.IsValid() &&
			(
				localPlayer.IsKillTimerEnabled ||
				localPlayer.ForceKillTimerContinue ||
				HudManager.Instance.UseButton.isActiveAndEnabled
			) &&
			MeetingHud.Instance == null &&
			ExileController.Instance == null &&
			IntroCutscene.Instance == null;
	}
}
