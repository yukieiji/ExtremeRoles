using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using UnityEngine;

namespace ExtremeRoles.Roles.Combination
{
    public class SupporterManager : FlexibleCombinationRoleManagerBase
    {
        public SupporterManager() : base(new Supporter(), 1)
        { }

    }

    public class Supporter : MultiAssignRoleBase, IRoleSpecialSetUp
    {
        private byte supportTarget;

        public Supporter(
            ) : base(
                ExtremeRoleId.Supporter,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Supporter.ToString(),
                ColorPalette.SupporterGreen,
                false, true, false, false)
        {}

        public void IntroBeginSetUp()
        {
            if (this.IsImposter())
            {
                this.RoleName = "Evil" + this.RoleName;
            }
            else if (this.IsCrewmate())
            {
                this.RoleName = "Nice" + this.RoleName;
            }


            List<byte> target = new List<byte>();

            foreach (var item in ExtremeRoleManager.GameRole)
            {
                if (item.Value.Id == this.Id) { continue; }

                if (((item.Value.Id == ExtremeRoleId.Marlin) && this.IsCrewmate()) ||
                    ((item.Value.Id == ExtremeRoleId.Assassin) && this.IsImposter()))
                {
                    target.Add(item.Key);
                }
            }

            if (target.Count == 0)
            {
                foreach (var item in ExtremeRoleManager.GameRole)
                {

                    if (item.Value.Id == this.Id) { continue; }

                    if ((item.Value.IsCrewmate() && this.IsCrewmate()) ||
                        (item.Value.IsImposter() && this.IsImposter()))
                    {
                        target.Add(item.Key);
                    }
                }
            }

            target = target.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            this.supportTarget = target[0];

        }

        public void IntroEndSetUp()
        {
            return;
        }


        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (targetPlayerId == this.supportTarget)
            {
                return targetRole.NameColor;
            }

            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override string GetIntroDescription()
        {
            var roleName = ExtremeRoleManager.GameRole[
                this.supportTarget].GetColoredRoleName();
            var playerName = Helper.Player.GetPlayerControlById(
                this.supportTarget).Data.PlayerName;

            return string.Format(
                base.GetIntroDescription(),
                playerName, roleName);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var imposterSetting = OptionsHolder.AllOption[
                GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter)];

            CreateKillerOption(imposterSetting);

        }

        protected override void RoleSpecificInit()
        {
            this.supportTarget = byte.MaxValue;
        }
    }
}
