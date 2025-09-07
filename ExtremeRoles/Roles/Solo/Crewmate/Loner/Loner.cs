using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Loner;

#nullable enable

public sealed class LonerRole : SingleRoleBase, IRoleUpdate, IRoleResetMeeting
{
    public enum Option
    {
        StressMaxGage,
		StressRange,
		StressIgnoreTime,
		StressProgressOnTask,
		StressProgressOnVentPlayer,
		StressProgressOnMovingPlatPlayer,
		IsShowArrow,
		ArrowNum,
		ArrowShowVentPlayer,
	}

    public override IStatusModel? Status => status;
    private LonerStatusModel? status;
	private LonerAbilityHandler? ability;

    public LonerRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Loner,
			ColorPalette.FencerPin),
        false, true, false, false)
    {
    }

    public void Update(PlayerControl rolePlayer)
    {
		this.ability?.Update(rolePlayer);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		factory.CreateIntOption(Option.StressMaxGage, 10, 1, 100, 1);
		factory.CreateFloatOption(Option.StressRange, 2.5f, 0.5f, 3.0f, 0.1f);
		factory.CreateIntOption(Option.StressIgnoreTime, 10, 1, 30, 1, format: OptionUnit.Second);
		factory.CreateBoolOption(Option.StressProgressOnTask, false);
		factory.CreateBoolOption(Option.StressProgressOnVentPlayer, true);
		factory.CreateBoolOption(Option.StressProgressOnMovingPlatPlayer, false);
		
		var arrowOpt = factory.CreateBoolOption(Option.IsShowArrow, true);
		factory.CreateIntOption(Option.ArrowNum, 1, 1, 5, 1);
		factory.CreateBoolOption(Option.ArrowShowVentPlayer, true);
	}

	protected override void RoleSpecificInit()
    {
		var loader = this.Loader;
        status = new LonerStatusModel(
			loader.GetValue<Option, float>(Option.StressRange),
			loader.GetValue<Option, float>(Option.StressIgnoreTime),
			new StressProgress.Option(
				loader.GetValue<Option, bool>(Option.StressProgressOnTask),
				loader.GetValue<Option, bool>(Option.StressProgressOnVentPlayer),
				loader.GetValue<Option, bool>(Option.StressProgressOnMovingPlatPlayer)));

		this.ability = new LonerAbilityHandler(
			loader.GetValue<Option, int>(Option.StressMaxGage),
			loader.GetValue<Option, bool>(Option.IsShowArrow) ? loader.GetValue<Option, int>(Option.ArrowNum) : 0,
			loader.GetValue<Option, bool>(Option.ArrowShowVentPlayer),
			status);
        AbilityClass = this.ability;
    }

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
		this.ability?.Reset();
	}
}
