using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination
{

    public class LoverManager : FlexibleCombinationRoleManagerBase
    {
        public LoverManager() : base(new Lover())
        { }

    }

    public class Lover : MultiAssignRoleBase
    {

        public enum LoverOption 
        {
            IsNeutral,
            BecomNeutral,
            DethWhenUnderAlive,
        }

        private bool becomeKiller = false;
        private int limit = 0;

        public Lover() : base(
            ExtremeRoleId.Lover,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Lover.ToString(),
            ColorPalette.LoverPink,
            false,
            true,
            false,
            false)
        {}

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            killedUpdate();
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            exiledUpdate();
        }

        public override string GetRolePlayerNameTag(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (targetRole.Id == ExtremeRoleId.Lover &&
                this.IsSameControlId(targetRole))
            {
                return Design.ColoedString(
                    ColorPalette.LoverPink, " ♥");
            }

            return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
        }

        public override string GetIntroDescription()
        {
            string baseString = base.GetIntroDescription();
            baseString += Design.ColoedString(
                ColorPalette.LoverPink, "\n♥");

            List<byte> lover = getAliveSameLover();
            baseString += Player.GetPlayerControlById(lover[0]).Data.PlayerName;
            if (lover.Count != 0)
            {
                for (int i = 1; i < lover.Count; ++i)
                {

                    if (i == 1)
                    {
                        baseString += Translation.GetString("AndFirst");
                    }
                    else
                    {
                        baseString += Translation.GetString("And");
                    }
                    baseString += Player.GetPlayerControlById(
                        lover[i]).Data.PlayerName;

                }
            }
            

            baseString += Translation.GetString("LoverIntoPlus") + Design.ColoedString(
                ColorPalette.LoverPink, "♥");

            return baseString;
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            if (targetRole.Id == ExtremeRoleId.Lover &&
                this.IsSameControlId(targetRole))
            {
                return ColorPalette.LoverPink;
            }

            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (targetRole.Id == ExtremeRoleId.Lover &&
                this.IsSameControlId(targetRole))
            {
                return true;
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        public static void ForceReplaceToNeutral(byte callerId, byte targetId)
        {
            var newKiller = ExtremeRoleManager.GameRole[targetId];
            newKiller.Team = ExtremeRoleType.Neutral;
            newKiller.CanKill = true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var neutralSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.IsNeutral),
                Design.ConcatString(
                    this.RoleName,
                    LoverOption.IsNeutral.ToString()),
                false, parentOps);

            var killerSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.BecomNeutral),
                Design.ConcatString(
                    this.RoleName,
                    LoverOption.BecomNeutral.ToString()),
                false, neutralSetting);

            var deathSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.DethWhenUnderAlive),
                Design.ConcatString(
                    this.RoleName,
                    LoverOption.DethWhenUnderAlive.ToString()),
                1, 1, 1, killerSetting,
                invert: true,
                enableCheckOption: parentOps);

            CreateKillerOption(killerSetting);

            OptionsHolder.AllOption[
                GetManagerOptionId(
                    CombinationRoleCommonOption.AssignsNum)].SetUpdateOption(deathSetting);

        }

        protected override void RoleSpecificInit()
        {

            bool isNeutral = OptionsHolder.AllOption[
                GetRoleOptionId((int)LoverOption.IsNeutral)].GetValue();

            this.becomeKiller = OptionsHolder.AllOption[
                GetRoleOptionId((int)LoverOption.BecomNeutral)].GetValue() && isNeutral;

            if (isNeutral && !this.becomeKiller)
            {
                this.Team = ExtremeRoleType.Neutral;
            }

            this.limit = OptionsHolder.AllOption[
                GetRoleOptionId((int)LoverOption.DethWhenUnderAlive)].GetValue();

        }

        private void exiledUpdate()
        {
            List<byte> alive = getAliveSameLover();

            if (this.becomeKiller)
            {
                if (alive.Count != 1) { return; }
                becomeAliveLoverToKiller(alive[0]);
            }
            else
            {
                if (alive.Count > limit) { return; }
                
                foreach (byte playerId in alive)
                {
                    var player = Player.GetPlayerControlById(playerId);
                    player.Exiled();
                }
            }
        }

        private void killedUpdate()
        {
            List<byte> alive = getAliveSameLover();
            if (this.becomeKiller)
            {
                if (alive.Count != 1) { return; }
                becomeAliveLoverToKiller(alive[0]);
                changeAllLoverToNeutral();

            }
            else
            {
                if (alive.Count > limit) { return; }

                foreach (byte playerId in alive)
                {
                    var player = Player.GetPlayerControlById(playerId);

                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.Data.IsDead = true;
                }
            }
        }
        private void changeAllLoverToNeutral()
        {
            foreach (var item in ExtremeRoleManager.GameRole)
            {
                if (this.IsSameControlId(item.Value))
                {
                    item.Value.Team = ExtremeRoleType.Neutral;
                }
            }
        }

        private void becomeAliveLoverToKiller(byte alivePlayerId)
        {

            PlayerControl rolePlayer = PlayerControl.LocalPlayer;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                rolePlayer.NetId,
                (byte)RPCOperator.Command.ReplaceRole,
                Hazel.SendOption.Reliable, -1);

            writer.Write(rolePlayer.PlayerId);
            writer.Write(alivePlayerId);
            writer.Write(
                (byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            ForceReplaceToNeutral(rolePlayer.PlayerId, alivePlayerId);
        }

        private List<byte> getAliveSameLover()
        {

            List<byte> alive = new List<byte>();

            foreach(var item in ExtremeRoleManager.GameRole)
            {
                if (this.IsSameControlId(item.Value) &&
                    !(GameData.Instance.GetPlayerById(item.Key).IsDead))
                {
                    alive.Add(item.Key);
                }
            }

            return alive;

        }
    }
}
