using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Fencer : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
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

    public bool IsCounter = false;
    public float Timer = 0.0f;
    public float MaxTime = 120f;

    private ExtremeAbilityButton takeTaskButton;

    public Fencer() : base(
        ExtremeRoleId.Fencer,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Fencer.ToString(),
        ColorPalette.FencerPin,
        false, true, false, false)
    { }

    public static void Ability(ref MessageReader reader)
    {
        byte rolePlayerId = reader.ReadByte();
        FencerAbility abilityType = (FencerAbility)reader.ReadByte();

        var fencer = ExtremeRoleManager.GetSafeCastedRole<Fencer>(rolePlayerId);
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
                enableKillButton(fencer, rolePlayerId);
                break;
            default:
                break;
        }

    }

    private static void counterOn(Fencer fencer)
    {
        fencer.IsCounter = true;
    }

    public static void counterOff(Fencer fencer)
    {
        fencer.IsCounter = false;
    }

    private static void enableKillButton(
        Fencer fencer, byte rolePlayerId)
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        if (localPlayer.PlayerId != rolePlayerId) { return; }

        if (MapBehaviour.Instance)
        {
            MapBehaviour.Instance.Close();
        }
        if (Minigame.Instance)
        {
            Minigame.Instance.ForceClose();
        }

        fencer.CanKill = true;
        localPlayer.killTimer = 0.1f;

        fencer.Timer = fencer.MaxTime;
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "counter",
			Resources.Loader.CreateSpriteFromResources(
				Path.FencerCounter),
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

    public override bool TryRolePlayerKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {

        if (this.IsCounter)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.FencerAbility))
            {
                caller.WriteByte(
                    rolePlayer.PlayerId);
                caller.WriteByte((byte)FencerAbility.ActivateKillButton);
            }
            enableKillButton(this, rolePlayer.PlayerId);
            Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.85f);
            return false;
        }

        return true;
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
        this.Timer = 0.0f;
        this.MaxTime = this.Loader.GetValue<FencerOption, float>(
            FencerOption.ResetTime);
    }
}
