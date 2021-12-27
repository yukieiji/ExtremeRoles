using System;
using System.Collections.Generic;

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
                if (item.Value.GameControlId == this.GameControlId)
                {
                    item.Value.Teams = ExtremeRoleType.Neutral;
                }
            }
        }

        private void becomeAliveLoverToKiller(byte alivePlayerId)
        {
            var newKiller = ExtremeRoleManager.GameRole[alivePlayerId];
            newKiller.Teams = ExtremeRoleType.Neutral;
            newKiller.CanKill = true;
        }

        private List<byte> getAliveSameLover()
        {

            List<byte> alive = new List<byte>();

            foreach(var item in ExtremeRoleManager.GameRole)
            {
                if (item.Value.GameControlId == this.GameControlId &&
                    !GameData.Instance.GetPlayerById(item.Key).IsDead)
                {
                    alive.Add(item.Key);
                }
            }

            return alive;

        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
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
                true, parentOps);

            var deathSetting = CustomOption.Create(
                GetRoleOptionId((int)LoverOption.DethWhenUnderAlive),
                Design.ConcatString(
                    this.RoleName,
                    LoverOption.DethWhenUnderAlive.ToString()),
                1, 1, 1, killerSetting, invert:true);

            CreateKillerOption(killerSetting);

            OptionsHolder.AllOptions[
                GetManagerOptionId(
                    CombinationRoleCommonOption.AssignsNum)].SetUpdateOption(deathSetting);

        }

        protected override void RoleSpecificInit()
        {

            if (OptionsHolder.AllOptions[
                    GetRoleOptionId((int)LoverOption.IsNeutral)].GetValue())
            {
                this.Teams = ExtremeRoleType.Neutral;
            }

            this.becomeKiller = OptionsHolder.AllOptions[
                GetRoleOptionId((int)LoverOption.BecomNeutral)].GetValue();
            this.limit = OptionsHolder.AllOptions[
                GetRoleOptionId((int)LoverOption.DethWhenUnderAlive)].GetValue();

        }
    }
}
