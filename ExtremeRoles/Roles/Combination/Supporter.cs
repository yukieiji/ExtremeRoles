using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Roles.Combination;

public sealed class SupporterManager : FlexibleCombinationRoleManagerBase
{
    public SupporterManager() : base(new Supporter(), 1)
    { }

}

public sealed class Supporter : MultiAssignRoleBase, IRoleSpecialSetUp
{
    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.RawRoleName);

    private byte supportTargetId;
    private string supportPlayerName;
    private string supportRoleName;
    private Color supportColor;

    private string roleNamePrefix;

    public Supporter(
        ) : base(
            ExtremeRoleId.Supporter,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Supporter.ToString(),
            ColorPalette.SupporterGreen,
            false, true, false, false,
            tab: OptionTab.Combination)
    {}

    public void IntroBeginSetUp()
    {
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
        Color supportColor = supportRole.GetNameColor();
        this.supportPlayerName = Player.GetPlayerControlById(
            this.supportTargetId).Data.PlayerName;
        this.supportColor = new Color(
            supportColor.r,
            supportColor.g,
            supportColor.b,
            supportColor.a);

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
        IOptionInfo parentOps)
    {
        var imposterSetting = AllOptionHolder.Instance.Get<bool>(
            GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter),
            AllOptionHolder.ValueType.Bool);

        CreateKillerOption(imposterSetting);

    }

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();
        this.supportTargetId = byte.MaxValue;
    }
}
