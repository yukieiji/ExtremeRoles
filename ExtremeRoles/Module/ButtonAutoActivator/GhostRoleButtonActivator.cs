using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ButtonAutoActivator
{
    public sealed class GhostRoleButtonActivator : IButtonAutoActivator
    {
        public static bool IsComSabNow()
        {
            return PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
                CachedPlayerControl.LocalPlayer);
        }

        public bool IsActive()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            return
                (
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue ||
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled
                ) &&
                localPlayer.Data != null &&
                MeetingHud.Instance == null &&
                ExileController.Instance == null &&
                localPlayer.Data.IsDead;
        }
    }
}
