using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using AmongUs.GameOptions;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Commander : SingleRoleBase, IRoleAbility
    {
        public RoleAbilityButtonBase Button
        { 
            get => this.commandAttackButton;
            set
            {
                this.commandAttackButton = value;
            }
        }

        public enum CommanderOption
        {
            KillCoolReduceTime,
            KillCoolReduceImpBonus,
            IncreaseKillNum
        }

        private RoleAbilityButtonBase commandAttackButton;
        private float killCoolReduceTime;
        private float killCoolImpNumBonus;
        private int increaseKillNum;
        private int killCount;

        public Commander() : base(
            ExtremeRoleId.Commander,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Commander.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public static void AttackCommad(byte rolePlayerId)
        {
            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (role == null || !role.IsImpostor()) { return; }

            Commander commander = ExtremeRoleManager.GetSafeCastedRole<Commander>(rolePlayerId);
            int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                Int32OptionNames.NumImpostors);
            int deadImpNum = maxImpNum;
            foreach (var (playerId, checkRole) in ExtremeRoleManager.GameRole)
            {
                if (!checkRole.IsImpostor()) { continue; }

                var player = GameData.Instance.GetPlayerById(playerId);

                if (player == null || player.IsDead || player.Disconnected) { continue; }

                --deadImpNum;
            }

            deadImpNum = Mathf.Clamp(deadImpNum, 0, maxImpNum);

            float killCool = CachedPlayerControl.LocalPlayer.PlayerControl.killTimer;
            if (killCool > 0.1f)
            {
                float newKillCool = killCool - 
                    commander.killCoolReduceTime - 
                    (commander.killCoolImpNumBonus * deadImpNum);

                CachedPlayerControl.LocalPlayer.PlayerControl.killTimer = Mathf.Clamp(
                    newKillCool, 0.1f, killCool);
            }
            Sound.PlaySound(
                Sound.SoundType.CommanderReduceKillCool, 1.2f);
        }
        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("attackCommand"),
                Loader.CreateSpriteFromResources(
                   Path.CommanderAttackCommand));
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {
            PlayerControl player = CachedPlayerControl.LocalPlayer;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.CommanderAttackCommand))
            {
                caller.WriteByte(player.PlayerId);
            }
            AttackCommad(player.PlayerId);

            return true;
        }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            ++this.killCount;
            this.killCount = this.killCount % this.increaseKillNum;
            if (this.killCount == 0)
            {
                var button = ((AbilityCountButton)this.commandAttackButton);
                button.UpdateAbilityCount(button.CurAbilityNum + 1);
            }
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateFloatOption(
                CommanderOption.KillCoolReduceTime,
                2.0f, 0.5f, 5.0f, 0.1f, parentOps,
                format: OptionUnit.Second);
            CreateFloatOption(
                CommanderOption.KillCoolReduceImpBonus,
                1.5f, 0.1f, 3.0f, 0.1f, parentOps,
                format: OptionUnit.Second);
            CreateIntOption(
                CommanderOption.IncreaseKillNum,
                2, 1, 3, 1, parentOps,
                format: OptionUnit.Shot);
            this.CreateAbilityCountOption(parentOps, 1, 3);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;
            this.killCoolReduceTime = allOpt[
                GetRoleOptionId(CommanderOption.KillCoolReduceTime)].GetValue();
            this.killCoolImpNumBonus = allOpt[
                GetRoleOptionId(CommanderOption.KillCoolReduceImpBonus)].GetValue();
            this.increaseKillNum = allOpt[
                GetRoleOptionId(CommanderOption.IncreaseKillNum)].GetValue();

            this.killCount = 0;
            this.RoleAbilityInit();
        }
    }
}
