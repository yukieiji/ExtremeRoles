using Hazel;

using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.TimeMaster;

#nullable enable

public sealed class TimeMasterRole : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum TimeMasterOption
    {
        RewindTime
    }

    public enum TimeMasterOps : byte
    {
        ShieldOff,
        ShieldOn,
        RewindTime,
        ResetMeeting,
    }

    public ExtremeAbilityButton? Button { get; set; }

	public override IStatusModel? Status => this.status;
	private TimeMasterStatusModel? status;

    public TimeMaster() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.TimeMaster,
			ColorPalette.TimeMasterBlue),
        false, true, false, false)
    {
    }

    public static void Ability(ref MessageReader reader)
    {
        byte tmPlayerId = reader.ReadByte();
        TimeMasterOps ops = (TimeMasterOps)reader.ReadByte();
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(tmPlayerId);
        if (timeMaster is null ||
			timeMaster.AbilityClass is not TimeMasterAbilityHandler timeMasterAbilityHandler)
		{
			return;
		}

        switch (ops)
        {
            case TimeMasterOps.ShieldOff:
                shieldOff(tmPlayerId);
                break;
            case TimeMasterOps.ShieldOn:
                shieldOn(tmPlayerId);
                break;
            case TimeMasterOps.RewindTime:
				timeMasterAbilityHandler.StartRewind(tmPlayerId);
				break;
            case TimeMasterOps.ResetMeeting:
				timeMasterAbilityHandler.ResetMeeting(tmPlayerId);
                break;
            default:
                break;
        }
    }

    private static void shieldOn(byte playerId)
    {
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(playerId);

        if (timeMaster is not null &&
			timeMaster.status is not null)
        {
            timeMaster.status.IsShieldOn = true;
        }
    }

    private static void shieldOff(byte playerId)
    {
        var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMasterRole>(playerId);

        if (timeMaster is not null && 
			timeMaster.status is not null)
        {
            timeMaster.status.IsShieldOn = false;
        }
    }

    public void CleanUp()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ShieldOff);
        }
        shieldOff(localPlayer.PlayerId);
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "timeShield",
			UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.TimeMasterTimeShield),
            abilityOff: CleanUp);
        Button?.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ShieldOn);
        }
        shieldOn(localPlayer.PlayerId);

        return true;
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingStart()
    {

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TimeMasterAbility))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WriteByte((byte)TimeMasterOps.ResetMeeting);
        }
		if (this.AbilityClass is TimeMasterAbilityHandler timeMasterAbilityHandler)
		{
			timeMasterAbilityHandler.ResetMeeting(localPlayer.PlayerId);
		}
	}

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory, 3.0f);

        factory.CreateFloatOption(
            TimeMasterOption.RewindTime,
            5.0f, 1.0f, 60.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
		this.status = new TimeMasterStatusModel(
			Loader.GetValue<TimeMasterOption, float>(TimeMasterOption.RewindTime));
		AbilityClass = new TimeMasterAbilityHandler(status);
    }
}
