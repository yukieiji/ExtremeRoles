using UnityEngine;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using AmongUs.GameOptions;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class PsychoKiller : SingleRoleBase, IRoleResetMeeting
{
    private bool isResetMeeting;
    private float reduceRate;
    private float defaultKillCoolTime;

    private int combMax;
    private int combCount;

    public enum PsychoKillerOption
    {
        KillCoolReduceRate,
        CombMax,
        CombResetWhenMeeting
    }

    public PsychoKiller() : base(
        ExtremeRoleId.PsychoKiller,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.PsychoKiller.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    {}

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        this.KillCoolTime = this.defaultKillCoolTime;
        if (this.isResetMeeting)
        {
            this.combCount = 1;
        }
        else
        {
            if(this.combCount >= this.combMax)
            {
                this.combCount = this.combMax;
            }
        }
    }

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            this.combCount - 1);
    }


    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (this.combMax >= this.combCount)
        {
            this.KillCoolTime = this.KillCoolTime * (
                (100f - (this.reduceRate * this.combCount)) / 100f);
            this.KillCoolTime = Mathf.Clamp(
                this.KillCoolTime, 0.1f, this.defaultKillCoolTime);
            ++this.combCount;
        }
        return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateIntOption(
            PsychoKillerOption.KillCoolReduceRate,
            5, 1, 15, 1, parentOps,
            format: OptionUnit.Percentage);

        CreateIntOption(
            PsychoKillerOption.CombMax,
            2, 1, 5, 1,
            parentOps);

        CreateBoolOption(
            PsychoKillerOption.CombResetWhenMeeting,
            true, parentOps);
    }

    protected override void RoleSpecificInit()
    {

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.KillCooldown);
        }

        var allOption = AllOptionHolder.Instance;

        this.reduceRate = allOption.GetValue<int>(
            GetRoleOptionId(PsychoKillerOption.KillCoolReduceRate));
        this.isResetMeeting = allOption.GetValue<bool>(
            GetRoleOptionId(PsychoKillerOption.CombResetWhenMeeting));
        this.combMax= allOption.GetValue<int>(
            GetRoleOptionId(PsychoKillerOption.CombMax));

        this.combCount = 1;
        this.defaultKillCoolTime = this.KillCoolTime;
    }
}
