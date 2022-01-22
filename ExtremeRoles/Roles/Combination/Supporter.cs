using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Combination
{
    public class SupporterManager : FlexibleCombinationRoleManagerBase
    {
        public SupporterManager() : base(new Supporter(), 1)
        { }

    }

    public class Supporter : MultiAssignRoleBase, IRoleSpecialSetUp
    {
        private byte supportTargetId;
        private string supportPlayerName;
        private string supportRoleName;
        private Color supportColor;

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
            if (this.IsImpostor())
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
                    ((item.Value.Id == ExtremeRoleId.Assassin) && this.IsImpostor()))
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
                        (item.Value.IsImpostor() && this.IsImpostor()))
                    {
                        target.Add(item.Key);
                    }
                }
            }

            target = target.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            this.supportTargetId = target[0];

            var supportRole = ExtremeRoleManager.GameRole[
                this.supportTargetId];

            this.supportRoleName = supportRole.GetColoredRoleName();
            this.supportPlayerName = Player.GetPlayerControlById(
                this.supportTargetId).Data.PlayerName;
            this.supportColor = new Color(
                supportRole.NameColor.r,
                supportRole.NameColor.g,
                supportRole.NameColor.b,
                supportRole.NameColor.a);

        }

        public void IntroEndSetUp()
        {
            return;
        }

        public override string GetFullDescription()
        {

            string baseDesc;

            if (this.IsImpostor())
            {
                baseDesc = Translation.GetString(
                    $"{this.Id}ImposterFullDescription");
            }
            else
            {
                baseDesc = base.GetFullDescription();
            }

            return string.Format(
                baseDesc,
                this.supportPlayerName,
                this.supportRoleName);
        }
        public override string GetRolePlayerNameTag(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (targetPlayerId == this.supportTargetId)
            {
                return Design.ColoedString(
                    ColorPalette.SupporterGreen,
                    " ★");
            }

            return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
        }


        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (targetPlayerId == this.supportTargetId)
            {
                return this.supportColor;
            }

            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override string GetIntroDescription()
        {
            return string.Format(
                base.GetIntroDescription(),
                Design.ColoedString(
                    Palette.White,
                    supportPlayerName),
                supportRoleName);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            var imposterSetting = OptionHolder.AllOption[
                GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter)];

            CreateKillerOption(imposterSetting);

        }

        protected override void RoleSpecificInit()
        {
            this.supportTargetId = byte.MaxValue;
        }
    }
}
