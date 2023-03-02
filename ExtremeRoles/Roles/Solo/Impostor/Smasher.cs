using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Smasher : SingleRoleBase, IRoleAbility
    {
        public enum SmasherOption
        {
            SmashPenaltyKillCool,
        }

        public ExtremeAbilityButton Button
        {
            get => this.smashButton;
            set
            {
                this.smashButton = value;
            }
        }

        private ExtremeAbilityButton smashButton;
        private byte targetPlayerId;
        private float prevKillCool;
        private float penaltyKillCool;

        public Smasher() : base(
            ExtremeRoleId.Smasher,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Smasher.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                "smash", FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite);
        }

        public bool IsAbilityUse()
        {
            this.targetPlayerId = byte.MaxValue;
            var player = Player.GetClosestPlayerInKillRange();
            if (player != null)
            {
                this.targetPlayerId = player.PlayerId;
            }
            return this.IsCommonUse() && this.targetPlayerId != byte.MaxValue;
        }

        public bool UseAbility()
        {
            PlayerControl killer = CachedPlayerControl.LocalPlayer;
            if (killer.Data.IsDead || !killer.CanMove) { return false; }

            var role = ExtremeRoleManager.GetLocalPlayerRole();
            var targetPlayerRole = ExtremeRoleManager.GameRole[this.targetPlayerId];
            var target = Player.GetPlayerControlById(this.targetPlayerId);

            if (target == null) { return false; }

            bool canKill = role.TryRolePlayerKillTo(
                killer, target);
            if (!canKill) { return false; }

            canKill = targetPlayerRole.TryRolePlayerKilledFrom(
                target, killer);
            if (!canKill) { return false; }

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    canKill = multiAssignRole.AnotherRole.TryRolePlayerKillTo(
                        killer, target);
                    if (!canKill) { return false; }
                }
            }

            multiAssignRole = targetPlayerRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    canKill = multiAssignRole.AnotherRole.TryRolePlayerKilledFrom(
                        target, killer);
                    if (!canKill) { return false; }
                }
            }

            if (Crewmate.BodyGuard.TryRpcKillGuardedBodyGuard(
                    killer.PlayerId, target.PlayerId))
            {
                featKillPenalty(killer);
                return true;
            }
            else if (Patches.Button.KillButtonDoClickPatch.IsMissMuderKill(
                killer, target))
            {
                return false;
            }

            this.prevKillCool = PlayerControl.LocalPlayer.killTimer;

            Player.RpcUncheckMurderPlayer(
                killer.PlayerId,
                target.PlayerId,
                byte.MaxValue);

            featKillPenalty(killer);
            return true;
        }

        private void featKillPenalty(PlayerControl killer)
        {
            if (this.penaltyKillCool > 0.0f)
            {

                this.HasOtherKillCool = true;
                API.Extension.State.RoleState.AddKillCoolOffset(
                    this.penaltyKillCool);
            }

            killer.killTimer = this.prevKillCool;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 1, 14);

            CreateFloatOption(
                SmasherOption.SmashPenaltyKillCool,
                4.0f, 0.0f, 30f, 0.5f, parentOps,
                format: OptionUnit.Second);

        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
            this.penaltyKillCool = OptionHolder.AllOption[
                GetRoleOptionId(SmasherOption.SmashPenaltyKillCool)].GetValue();
        }

        public void ResetOnMeetingStart()
        {
            return;
        }

        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
        {
            return;
        }
    }
}
