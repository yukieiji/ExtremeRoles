using System;
using System.Collections.Generic;

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

        public override string GetFullDescription()
        {
            string baseDesc;

            if (this.IsImpostor() && !this.CanHasAnotherRole)
            {
                baseDesc = Translation.GetString($"{this.Id}ImposterFullDescription");
            }
            else if (this.CanKill && !this.CanHasAnotherRole)
            {
                baseDesc = Translation.GetString($"{this.Id}NeutralKillerFullDescription");
            }
            else if (this.IsNeutral() && !this.CanHasAnotherRole)
            {
                baseDesc = Translation.GetString($"{this.Id}NeutralFullDescription");
            }
            else
            {
                baseDesc = base.GetFullDescription();
            }

            baseDesc = $"{baseDesc}\n{Translation.GetString("curLover")}:";

            foreach (var item in ExtremeRoleManager.GameRole)
            {
                if (this.IsSameControlId(item.Value))
                {
                    string playerName = Player.GetPlayerControlById(
                        item.Key).Data.PlayerName;
                    baseDesc += $"{playerName},"; ;
                }
            }

            return baseDesc;
        }

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

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if (!this.CanKill || this.IsImpostor() || this.CanHasAnotherRole)
            {
                return base.GetImportantText(isContainFakeTask);
            }

            string killerText = Design.ColoedString(
                this.NameColor,
                $"{this.GetColoredRoleName()}: {Translation.GetString($"{this.Id}KillerShortDescription")}");

            if (this.AnotherRole == null)
            {
                return this.getTaskText(
                    killerText, isContainFakeTask);
            }

            string anotherRoleString = this.AnotherRole.GetImportantText(false);

            killerText = $"{killerText}\r\n{anotherRoleString}";

            return this.getTaskText(
                killerText, isContainFakeTask);
        }

        public override string GetIntroDescription()
        {
            string baseString = base.GetIntroDescription();
            baseString += Design.ColoedString(
                ColorPalette.LoverPink, "\n♥ ");

            List<byte> lover = getAliveSameLover();

            lover.Remove(PlayerControl.LocalPlayer.PlayerId);

            byte firstLover = lover[0];
            lover.RemoveAt(0);

            baseString += Player.GetPlayerControlById(
                firstLover).Data.PlayerName;
            if (lover.Count != 0)
            {
                for (int i = 0; i < lover.Count; ++i)
                {

                    if (i == 0)
                    {
                        baseString += Translation.GetString("andFirst");
                    }
                    else
                    {
                        baseString += Translation.GetString("and");
                    }
                    baseString += Player.GetPlayerControlById(
                        lover[i]).Data.PlayerName;

                }
            }

            return string.Concat(
                baseString,
                Translation.GetString("LoverIntoPlus"),
                Design.ColoedString(
                    ColorPalette.LoverPink, " ♥"));
        }


        public override string GetRoleTag() => "♥";

        public override string GetRolePlayerNameTag(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (targetRole.Id == ExtremeRoleId.Lover &&
                this.IsSameControlId(targetRole))
            {
                return Design.ColoedString(
                    ColorPalette.LoverPink,
                    $" {GetRoleTag()}");
            }

            return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
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
            var newKiller = (Lover)ExtremeRoleManager.GameRole[targetId];
            newKiller.Team = ExtremeRoleType.Neutral;
            newKiller.CanKill = true;
            newKiller.HasTask = false;
            newKiller.ChangeAllLoverToNeutral();
            ExtremeRoleManager.GameRole[targetId] = newKiller;
        }

        public void ChangeAllLoverToNeutral()
        {
            foreach (var item in ExtremeRoleManager.GameRole)
            {
                if (this.IsSameControlId(item.Value))
                {
                    item.Value.Team = ExtremeRoleType.Neutral;
                }
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var neutralSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.IsNeutral),
                string.Concat(
                    this.RoleName,
                    LoverOption.IsNeutral.ToString()),
                false, parentOps);

            var killerSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.BecomNeutral),
                string.Concat(
                    this.RoleName,
                    LoverOption.BecomNeutral.ToString()),
                false, neutralSetting);

            var deathSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.DethWhenUnderAlive),
                string.Concat(
                    this.RoleName,
                    LoverOption.DethWhenUnderAlive.ToString()),
                1, 1, 1, killerSetting,
                invert: true,
                enableCheckOption: parentOps);

            CreateKillerOption(killerSetting);

            OptionHolder.AllOption[
                GetManagerOptionId(
                    CombinationRoleCommonOption.AssignsNum)].SetUpdateOption(deathSetting);

        }

        protected override void RoleSpecificInit()
        {

            bool isNeutral = OptionHolder.AllOption[
                GetRoleOptionId((int)LoverOption.IsNeutral)].GetValue();

            this.becomeKiller = OptionHolder.AllOption[
                GetRoleOptionId((int)LoverOption.BecomNeutral)].GetValue() && isNeutral;

            if (isNeutral && !this.becomeKiller)
            {
                this.Team = ExtremeRoleType.Neutral;
            }

            this.limit = OptionHolder.AllOption[
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
            
            }
            else
            {
                if (alive.Count > limit) { return; }

                foreach (byte playerId in alive)
                {
                    var player = Player.GetPlayerControlById(playerId);
                    player.RpcMurderPlayer(player);
                }
            }
        }

        private void becomeAliveLoverToKiller(byte alivePlayerId)
        {

            PlayerControl rolePlayer = PlayerControl.LocalPlayer;
            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.ReplaceRole,
                new List<byte>
                { 
                    rolePlayer.PlayerId,
                    alivePlayerId,
                    (byte)ExtremeRoleManager.ReplaceOperation.LoverBecomeToNeutral,
                });

            ForceReplaceToNeutral(rolePlayer.PlayerId, alivePlayerId);
        }

        private string getTaskText(string baseString, bool isContainFakeTask)
        {
            if (isContainFakeTask)
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = $"{baseString}\r\n{fakeTaskString}";
            }

            return baseString;

        }

        private List<byte> getAliveSameLover()
        {

            List<byte> alive = new List<byte>();

            foreach(var item in ExtremeRoleManager.GameRole)
            {
                if (this.IsSameControlId(item.Value) && !(GameData.Instance.GetPlayerById(
                    item.Key).IsDead))
                {
                    alive.Add(item.Key);
                }
            }

            return alive;

        }
    }
}
