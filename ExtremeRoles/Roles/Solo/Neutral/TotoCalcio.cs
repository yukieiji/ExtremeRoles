using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;

using ExtremeRoles.Module.AbilityButton.Roles;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Totocalcio : SingleRoleBase, IRoleAbility, IRoleWinPlayerModifier
    {
        public enum TotocalcioOption
        {
            Range,
            FinalCoolTime,
        }

        private static HashSet<ExtremeRoleId> ignoreRole = new HashSet<ExtremeRoleId>()
        {
            ExtremeRoleId.Yoko,
            ExtremeRoleId.Vigilante,
        };

        public RoleAbilityButtonBase Button
        { 
            get => this.betButton;
            set
            {
                this.betButton = value;
            }
        }

        private RoleAbilityButtonBase betButton;
        
        private float range;
        private GameData.PlayerInfo betPlayer;
        private PlayerControl tmpTarget;

        private float defaultCoolTime;
        private float finalCoolTime;

        public Totocalcio() : base(
           ExtremeRoleId.Totocalcio,
           ExtremeRoleType.Neutral,
           ExtremeRoleId.Totocalcio.ToString(),
           ColorPalette.TotocalcioGreen,
           false, false, false, false)
        { }


        public static void SetBetTarget(
            byte rolePlayerId, byte betTargetPlayerId)
        {
            var totocalcio =  ExtremeRoleManager.GetSafeCastedRole<Totocalcio>(rolePlayerId);
            
            if (totocalcio != null)
            {
                totocalcio.betPlayer = GameData.Instance.GetPlayerById(betTargetPlayerId);
            }
        }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Helper.Translation.GetString("betPlayer"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton));
        }

        public bool IsAbilityUse()
        {
            this.tmpTarget = Helper.Player.GetPlayerTarget(
                CachedPlayerControl.LocalPlayer, this,
                this.range);
            return this.IsCommonUse() && this.tmpTarget != null;
        }

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            ref Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner,
            ref List<GameData.PlayerInfo> pulsWinner)
        {
            if (this.betPlayer == null) { return; }

            if (ignoreRole.Contains(
                ExtremeRoleManager.GameRole[
                    this.betPlayer.PlayerId].Id)) { return; }
            
            foreach (var win in winner.GetFastEnumerator())
            {
                if (win.PlayerName == this.betPlayer.PlayerName)
                {
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                    return;
                }
            }

            foreach (var win in pulsWinner)
            {
                if (win.PlayerName == this.betPlayer.PlayerName)
                {
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                    return;
                }
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.Button == null) { return; }

            int aliveNum = 0;
            
            foreach (var player in 
                GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (player.IsDead || player.Disconnected) { ++aliveNum; }
            }

            this.Button.SetAbilityCoolTime(
                this.defaultCoolTime + (
                    (this.finalCoolTime - this.defaultCoolTime) * (aliveNum / GameData.Instance.AllPlayers.Count)));
            this.Button.ResetCoolTimer();
        }

        public bool UseAbility()
        {
            if (this.tmpTarget == null) { return false; }

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.TotocalcioSetBetPlayer,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    this.tmpTarget.PlayerId,
                });
            SetBetTarget(
                CachedPlayerControl.LocalPlayer.PlayerId,
                this.tmpTarget.PlayerId);
            this.tmpTarget = null;
            return true;
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        public override string GetFullDescription() => 
            string.Format(
                base.GetFullDescription(),
                this.betPlayer != null ? 
                    this.betPlayer.PlayerName : Helper.Translation.GetString("loseNow"));

        public override string GetRolePlayerNameTag(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (this.betPlayer == null) { return ""; }

            if (targetPlayerId == this.betPlayer.PlayerId)
            {
                return Helper.Design.ColoedString(
                    this.NameColor, $" ▲");
            }

            return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CreateFloatOption(
                TotocalcioOption.Range,
                1.0f, 0.0f, 2.0f, 0.1f,
                parentOps);

            this.CreateCommonAbilityOption(parentOps);

            CreateFloatOption(
                TotocalcioOption.FinalCoolTime,
                80.0f, 45.0f, 180.0f, 0.1f,
                parentOps, format: OptionUnit.Second);
        }

        protected override void RoleSpecificInit()
        {
            this.betPlayer = null;
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(TotocalcioOption.Range)].GetValue();
            this.defaultCoolTime = OptionHolder.AllOption[
                GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue();
            this.finalCoolTime = OptionHolder.AllOption[
                GetRoleOptionId(TotocalcioOption.FinalCoolTime)].GetValue();
        }
    }
}
