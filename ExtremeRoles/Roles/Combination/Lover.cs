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
            BecomeNeutralLoverHasOtherVison,
            BecomeNeutralLoverVison,
            BecomeNeutralLoverApplyEnvironmentVisionEffect,
            BecomeNeutralLoverCanUseVent,
            DethWhenUnderAlive,
        }

        private bool becomeKiller = false;
        private int limit = 0;

        private bool killerLoverHasOtherVison = false;
        private float killerLoverVison = 0.0f;
        private bool killerLoverIsApplyEnvironmentVisionEffect = false;
        private bool killerLoverCanUseVent = false;

        public Lover() : base(
            ExtremeRoleId.Lover,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Lover.ToString(),
            ColorPalette.LoverPink,
            false, true,
            false, false)
        { }

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
            killedUpdate(rolePlayer);
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            exiledUpdate(rolePlayer);
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

        public void ChangeAllLoverToNeutral()
        {
            foreach (var item in ExtremeRoleManager.GameRole)
            {
                if (this.IsSameControlId(item.Value))
                {
                    item.Value.Team = ExtremeRoleType.Neutral;
                    item.Value.HasTask = false;
                }
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var neutralSetting = CreateBoolOption(
                LoverOption.IsNeutral,
                false, parentOps);

            var killerSetting = CreateBoolOption(
                LoverOption.BecomNeutral,
                false, neutralSetting);

            var deathSetting = CreateIntDynamicOption(
                LoverOption.DethWhenUnderAlive,
                1, 1, 1, killerSetting,
                invert: true,
                enableCheckOption: parentOps);

            CreateKillerOption(killerSetting);
            killerVisionSetting(killerSetting);

            CreateBoolOption(
                LoverOption.BecomeNeutralLoverCanUseVent,
                false, killerSetting);

            OptionHolder.AllOption[
                GetManagerOptionId(
                    CombinationRoleCommonOption.AssignsNum)].SetUpdateOption(deathSetting);

        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;

            bool isNeutral = allOption[
                GetRoleOptionId(LoverOption.IsNeutral)].GetValue();

            this.becomeKiller = allOption[
                GetRoleOptionId(LoverOption.BecomNeutral)].GetValue() && isNeutral;

            if (isNeutral && !this.becomeKiller)
            {
                this.Team = ExtremeRoleType.Neutral;
            }
            if (this.becomeKiller)
            {
                var baseOption = PlayerControl.GameOptions;

                this.HasOtherKillCool = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillCool)].GetValue();
                if (this.HasOtherKillCool)
                {
                    this.KillCoolTime = allOption[
                        GetRoleOptionId(KillerCommonOption.KillCoolDown)].GetValue();
                }
                else
                {
                    this.KillCoolTime = baseOption.KillCooldown;
                }

                this.HasOtherKillRange = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillRange)].GetValue();

                if (this.HasOtherKillRange)
                {
                    this.KillRange = allOption[
                        GetRoleOptionId(KillerCommonOption.KillRange)].GetValue();
                }
                else
                {
                    this.KillRange = baseOption.KillDistance;
                }

                this.killerLoverHasOtherVison = allOption[
                    GetRoleOptionId(LoverOption.BecomeNeutralLoverHasOtherVison)].GetValue();
                if (this.killerLoverHasOtherVison)
                {
                    this.killerLoverVison = allOption[
                        GetRoleOptionId(LoverOption.BecomeNeutralLoverVison)].GetValue();
                    this.killerLoverIsApplyEnvironmentVisionEffect = allOption[
                        GetRoleOptionId(LoverOption.BecomeNeutralLoverApplyEnvironmentVisionEffect)].GetValue();
                }
                else
                {
                    this.killerLoverVison = this.Vison;
                    this.killerLoverIsApplyEnvironmentVisionEffect = this.IsApplyEnvironmentVision;
                }

                this.killerLoverCanUseVent = allOption[
                    GetRoleOptionId(LoverOption.BecomeNeutralLoverCanUseVent)].GetValue();

            }

            this.limit = allOption[
                GetRoleOptionId(LoverOption.DethWhenUnderAlive)].GetValue();

        }

        private void killerVisionSetting(
            CustomOptionBase killerOpt)
        {
            var visonOption = CreateBoolOption(
                LoverOption.BecomeNeutralLoverHasOtherVison,
                false, killerOpt);
            CreateFloatOption(LoverOption.BecomeNeutralLoverVison,
                2f, 0.25f, 5.0f, 0.25f,
                visonOption, format: OptionUnit.Multiplier);

            CreateBoolOption(
                LoverOption.BecomeNeutralLoverApplyEnvironmentVisionEffect,
                false, visonOption);
        }

        private void exiledUpdate(
            GameData.PlayerInfo exiledPlayer)
        {
            List<byte> alive = getAliveSameLover();
            alive.Remove(exiledPlayer.PlayerId);

            if (this.becomeKiller)
            {
                if (alive.Count != 1) { return; }
                forceReplaceToNeutral(alive[0]);
            }
            else
            {
                if (alive.Count > limit) { return; }
                
                foreach (byte playerId in alive)
                {
                    var player = Player.GetPlayerControlById(playerId);
                    if (player != null)
                    {
                        player.Exiled();
                    }
                }
            }
        }

        private void killedUpdate(PlayerControl killedPlayer)
        {
            List<byte> alive = getAliveSameLover();
            alive.Remove(killedPlayer.PlayerId);

            if (this.becomeKiller)
            {
                if (alive.Count != 1) { return; }
                forceReplaceToNeutral(alive[0]);
            
            }
            else
            {
                if (alive.Count > limit) { return; }

                foreach (byte playerId in alive)
                {
                    var player = Player.GetPlayerControlById(playerId);
                    if (player != null)
                    {
                        player.MurderPlayer(player);
                    }
                }
            }
        }

        private void forceReplaceToNeutral(byte targetId)
        {
            var newKiller = (Lover)ExtremeRoleManager.GameRole[targetId];
            newKiller.Team = ExtremeRoleType.Neutral;
            newKiller.CanKill = true;
            newKiller.HasTask = false;
            newKiller.HasOtherVison = newKiller.killerLoverHasOtherVison;
            newKiller.Vison = newKiller.killerLoverVison;
            newKiller.IsApplyEnvironmentVision = newKiller.killerLoverIsApplyEnvironmentVisionEffect;
            newKiller.UseVent = newKiller.killerLoverCanUseVent;
            newKiller.ChangeAllLoverToNeutral();
            ExtremeRoleManager.GameRole[targetId] = newKiller;
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
                var player = GameData.Instance.GetPlayerById(item.Key);
                if (this.IsSameControlId(item.Value) && 
                    (!player.IsDead || !player.Disconnected))
                {
                    alive.Add(item.Key);
                }
            }

            return alive;

        }
    }
}
