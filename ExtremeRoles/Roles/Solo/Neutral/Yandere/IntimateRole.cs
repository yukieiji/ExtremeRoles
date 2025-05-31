using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;

public sealed class IntimateRole : SingleRoleBase, IRoleWinPlayerModifier, IRoleUpdate
{
	public enum Option
	{
		CanKill,
		IsSubTeam,
		UseVent,
		HasTask,
		SeeYandereTaskRate,
	}

	public override IStatusModel? Status => this.status;

	private IntimateStatus? status;

	public IntimateRole() : base(
		ExtremeRoleId.Intimate,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Intimate.ToString(),
		ColorPalette.YandereVioletRed,
		false, false, false, false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case (GameOverReason)RoleGameOverReason.YandereKillAllOther:
			case (GameOverReason)RoleGameOverReason.YandereShipJustForTwo:
				winner.AddPool(rolePlayerInfo);
				break;
			default:
				break;
		}
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.Update(rolePlayer);
	}

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeYandere(targetRole) ?
				ColorPalette.YandereVioletRed :
				base.GetTargetRoleSeeColor(targetRole, targetPlayerId);

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		var killOpt = factory.CreateBoolOption(
			Option.CanKill, true);
		factory.CreateBoolOption(
			Option.IsSubTeam, true, killOpt, invert: true);
		factory.CreateBoolOption(
			Option.UseVent, false);
		var taskOpt = factory.CreateBoolOption(
			Option.HasTask, false);
		factory.CreateIntOption(
			Option.SeeYandereTaskRate, 50, 0, 100, 10,
			taskOpt, format: OptionUnit.Percentage);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.CanKill = loader.GetValue<Option, bool>(Option.CanKill);
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.HasTask = loader.GetValue<Option, bool>(Option.HasTask);
		this.status = new IntimateStatus(
			this.HasTask,
			loader.GetValue<Option, int>(Option.SeeYandereTaskRate),
			this.CanKill && loader.GetValue<Option, bool>(Option.IsSubTeam));
	}

	private bool canSeeYandere(SingleRoleBase targetRole)
			=>
				this.status is not null &&
				targetRole.Id is ExtremeRoleId.Yandere &&
				this.status.SeeYandere;

}
