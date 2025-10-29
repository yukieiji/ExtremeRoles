using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using UnityEngine;

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

	private struct StressInfo()
	{
		public int Cur { get; set; } = 0;
		public int Max { get; set; } = 0;
	}

    public override IStatusModel? Status => status;
    private LonerStatusModel? status;
	private LonerAbilityHandler? ability;
	private StressInfo info = new StressInfo();

    public LonerRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Loner,
			ColorPalette.LonerMidnightblue),
        false, true, false, false)
    {
    }

    public void Update(PlayerControl rolePlayer)
    {
		this.ability?.Update(rolePlayer);
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		var factory = categoryScope.Builder;
		factory.CreateIntOption(Option.StressMaxGage, 10, 1, 100, 1);
		factory.CreateFloatOption(Option.StressRange, 2.5f, 0.5f, 3.0f, 0.1f);
		factory.CreateIntOption(Option.StressIgnoreTime, 10, 1, 30, 1, format: OptionUnit.Second);
		factory.CreateBoolOption(Option.StressProgressOnTask, false);
		factory.CreateBoolOption(Option.StressProgressOnVentPlayer, true);
		factory.CreateBoolOption(Option.StressProgressOnMovingPlatPlayer, false);
		
		var arrowOpt = factory.CreateBoolOption(Option.IsShowArrow, true);
		factory.CreateIntOption(Option.ArrowNum, 1, 1, 5, 1, arrowOpt, invert: true);
		factory.CreateBoolOption(Option.ArrowShowVentPlayer, true, arrowOpt, invert: true);
	}

	protected override void RoleSpecificInit()
    {
		var loader = this.Loader;
        status = new LonerStatusModel(
			loader.GetValue<Option, float>(Option.StressRange),
			loader.GetValue<Option, int>(Option.StressIgnoreTime),
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

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			updateStressInfo();
			return $"({this.info.Cur}/{this.info.Max})";
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	public override string GetFullDescription()
	{
		updateStressInfo();
		return string.Format(
			base.GetFullDescription(),
			this.info.Cur, this.info.Max);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
		this.ability?.Reset();
	}

	private void updateStressInfo()
	{
		if (this.ability is null || this.status is null)
		{
			return;
		}

		this.info.Cur = Mathf.CeilToInt(this.status.StressGage);
		this.info.Max = Mathf.CeilToInt(this.ability.MaxStressGage);
	}
}
