using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate.Fencer;

public sealed class FencerRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
    public enum FencerOption
    {
        ResetTime
    }

    public enum FencerAbility : byte
    {
        CounterOn,
        CounterOff,
        ActivateKillButton
    }

    public ExtremeAbilityButton Button
    {
        get => this.takeTaskButton;
        set
        {
            this.takeTaskButton = value;
        }
    }

    private ExtremeAbilityButton takeTaskButton;

    public override IStatusModel? Status => status;
    private FencerStatusModel status;

    public override bool CanKill { get => status.CanKill; }

    public FencerRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Fencer,
			ColorPalette.FencerPin),
        false, true, false, false)
    {
    }

    public static void Ability(ref MessageReader reader)
    {
        byte rolePlayerId = reader.ReadByte();
        FencerAbility abilityType = (FencerAbility)reader.ReadByte();

        var fencer = ExtremeRoleManager.GetSafeCastedRole<FencerRole>(rolePlayerId);
        if (fencer == null) { return; }

        switch (abilityType)
        {
            case FencerAbility.CounterOn:
                counterOn(fencer);
                break;
            case FencerAbility.CounterOff:
                counterOff(fencer);
                break;
            case FencerAbility.ActivateKillButton:
                ((FencerAbilityHandler)fencer.AbilityClass!).EnableKillButton(rolePlayerId);
                break;
            default:
                break;
        }

    }

    private static void counterOn(FencerRole fencer)
    {
        fencer.status.IsCounter = true;
    }

    public static void counterOff(FencerRole fencer)
    {
        fencer.status.IsCounter = false;
    }

    public void CreateAbility()
    {
        this.CreateActivatingAbilityCountButton(
            "counter",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.FencerCounter),
            abilityOff: this.CleanUp,
            isReduceOnActive: true);
        this.Button.SetLabelToCrewmate();
    }
    public void CleanUp()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.FencerAbility))
        {
            caller.WriteByte(
                PlayerControl.LocalPlayer.PlayerId);
            caller.WriteByte((byte)FencerAbility.CounterOff);
        }
        counterOff(this);
    }

    public bool UseAbility()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.FencerAbility))
        {
            caller.WriteByte(
                PlayerControl.LocalPlayer.PlayerId);
            caller.WriteByte((byte)FencerAbility.CounterOn);
        }
        counterOn(this);
        return true;
    }

    public bool IsAbilityUse()
    {
        return IRoleAbility.IsCommonUse();
    }

    public void ResetOnMeetingStart()
    {
        this.CleanUp();
        this.CanKill = false;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (this.Timer <= 0.0f)
        {
            this.CanKill = false;
            return;
        }

        this.Timer -= Time.deltaTime;

    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 7, 3.0f);
        factory.CreateFloatOption(
            FencerOption.ResetTime,
            5.0f, 2.5f, 30.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        status = new FencerStatusModel(this.Loader.GetValue<FencerOption, float>(
            FencerOption.ResetTime));
        AbilityClass = new FencerAbilityHandler(status);
        status.Timer = 0.0f;
    }

    public void ResetOnMeetingStart()
    {
        this.CleanUp();
        status.CanKill = false;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (status.Timer <= 0.0f)
        {
            if (status.CanKill)
            {
                status.CanKill = false;
            }
            return;
        }

        status.Timer -= Time.deltaTime;
    }
}
