using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Module;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Solo.Crewmate.Fencer
{
    public class FencerAbilityHandler : IAbility, IKilledFrom
    {
        private FencerStatusModel status;

        public FencerAbilityHandler(FencerStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            if (status.IsCounter)
            {
                using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FencerAbility))
                {
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte((byte)FencerRole.FencerAbility.ActivateKillButton);
                }
                EnableKillButton(rolePlayer.PlayerId);
                Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.85f);
                return false;
            }

            return true;
        }

        public void EnableKillButton(byte rolePlayerId)
        {
            PlayerControl localPlayer = PlayerControl.LocalPlayer;

            if (localPlayer.PlayerId != rolePlayerId) { return; }

            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }

            status.CanKill = true;
            localPlayer.killTimer = 0.1f;

            status.Timer = status.MaxTime;
        }
    }
}
