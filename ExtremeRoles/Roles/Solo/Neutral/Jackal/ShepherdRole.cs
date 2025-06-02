using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public sealed class ShepherdRole : SingleRoleBase, IRoleWinPlayerModifier, IRoleUpdate
{
	public enum Option
	{
		CanKill,
		IsSubTeam,
		UseVent,
		HasTask,
		SeeJackalTaskRate,
	}

	public override IStatusModel? Status => this.status;

	private ShepherdStatus? status;

	public ShepherdRole() : base(
		ExtremeRoleId.Shepherd,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Shepherd.ToString(),
		ColorPalette.JackalBlue,
		false, false, false, false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case (GameOverReason)RoleGameOverReason.JackalKillAllOther:
				winner.AddPool(rolePlayerInfo);
				break;
			default:
				break;
		}
	}

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeJackal(targetRole) ?
				ColorPalette.JackalBlue :
				base.GetTargetRoleSeeColor(targetRole, targetPlayerId);

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		var killOpt = factory.CreateBoolOption(
			Option.CanKill, true);
		CreateKillerOption(factory, killOpt, true, true);
		factory.CreateBoolOption(
			Option.IsSubTeam, true, killOpt, invert: true);
		factory.CreateBoolOption(Option.UseVent, false);

		var taskOpt = factory.CreateBoolOption(
			Option.HasTask, false);
		factory.CreateIntOption(
			Option.SeeJackalTaskRate, 50, 0, 100, 10,
			taskOpt, format: OptionUnit.Percentage);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.CanKill = loader.GetValue<Option, bool>(Option.CanKill);
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.HasTask = loader.GetValue<Option, bool>(Option.HasTask);
		this.status = new ShepherdStatus(
			this.HasTask,
			loader.GetValue<Option, int>(Option.SeeJackalTaskRate),
			this.CanKill && loader.GetValue<Option, bool>(Option.IsSubTeam));
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.Update(rolePlayer);
	}
	private bool canSeeJackal(SingleRoleBase targetRole)
		=>
			this.status is not null &&
			targetRole.Id is ExtremeRoleId.Jackal &&
			this.status.SeeJackal;
}
