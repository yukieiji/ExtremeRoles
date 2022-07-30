using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Madmate : SingleRoleBase, IRoleAbility, IRoleUpdate, IRoleSpecialSetUp, IRoleWinPlayerModifier
    {
        public enum MadmateOption
        {
            CanUseVent,
            CanMoveVentToVent,
            HasTask,
            SeeImpostorTaskGage,
            CanSeeFromImpostor,
            CanSeeFromImpostorTaskGage,
        }

        private bool canMoveVentToVent = false;
        private bool canSeeFromImpostor = false;

        private bool isSeeImpostorNow = false;

        public RoleAbilityButtonBase Button
        {
            get => this.madmateAbilityButton;
            set
            {
                this.madmateAbilityButton = value;
            }
        }

        private RoleAbilityButtonBase madmateAbilityButton;

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Helper.Translation.GetString("selfKill"),
                Loader.CreateSpriteFromResources(
                    Path.SucideSprite));
        }

        public bool UseAbility()
        {

            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte> { playerId, playerId, byte.MaxValue });
            RPCOperator.UncheckedMurderPlayer(
                playerId,
                playerId,
                byte.MaxValue);
            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            if (!this.canMoveVentToVent) { return; }
        }

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            ref Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner,
            ref List<GameData.PlayerInfo> pulsWinner)
        {
            switch (reason)
            {
                case GameOverReason.ImpostorByVote:
                case GameOverReason.ImpostorByKill:
                case GameOverReason.ImpostorBySabotage:
                case GameOverReason.ImpostorDisconnect:
                case (GameOverReason)RoleGameOverReason.AssassinationMarin:
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                    break;
                default:
                    break;
            }
        }

        public void Update(PlayerControl rolePlayer)
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            var ventUseOpt = CreateBoolOption(
                MadmateOption.CanUseVent,
                false, parentOps);
            CreateBoolOption(
                MadmateOption.CanUseVent,
                false, ventUseOpt);
            var taskOpt = CreateBoolOption(
                MadmateOption.HasTask,
                false, parentOps);
            CreateIntOption(
                MadmateOption.SeeImpostorTaskGage,
                70, 0, 100, 10,
                taskOpt,
                format: OptionUnit.Percentage);
            var impFromSeeOpt = CreateBoolOption(
                MadmateOption.CanSeeFromImpostor,
                false, parentOps);
            CreateIntOption(
                MadmateOption.CanSeeFromImpostorTaskGage,
                70, 0, 100, 10,
                impFromSeeOpt,
                format: OptionUnit.Percentage);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;
            this.UseVent = allOpt[
                GetRoleOptionId(MadmateOption.CanUseVent)].GetValue();
            this.canMoveVentToVent = allOpt[
                GetRoleOptionId(MadmateOption.CanMoveVentToVent)].GetValue();
            this.HasTask = allOpt[
                GetRoleOptionId(MadmateOption.HasTask)].GetValue();
        }
    }
}
