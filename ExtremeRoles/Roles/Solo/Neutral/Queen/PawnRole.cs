using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class PawnRole : SingleRoleBase, IRoleWinPlayerModifier, IRoleUpdate
{
	public enum Option
	{
		UseVent,
		HasTask,
		SeeQueenTaskRate,
	}

	public override IStatusModel? Status => this.status;

	private PawnStatus? status;

	public PawnRole() : base(
		ExtremeRoleId.Pawn,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Pawn.ToString(),
		ColorPalette.QueenWhite,
		false, false, false, false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case (GameOverReason)RoleGameOverReason.QueenKillAllOther:
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

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeQueen(targetRole) ?
				Design.ColoedString(
					ColorPalette.QueenWhite,
					$" {QueenRole.RoleShowTag}") :
				base.GetRolePlayerNameTag(targetRole, targetPlayerId);

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeQueen(targetRole) ?
				ColorPalette.QueenWhite :
				base.GetTargetRoleSeeColor(targetRole, targetPlayerId);

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			Option.UseVent, false);
		var taskOpt = factory.CreateBoolOption(
			Option.HasTask, false);
		factory.CreateIntOption(
			Option.SeeQueenTaskRate, 50, 0, 100, 10,
			taskOpt, format: OptionUnit.Percentage);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.HasTask = loader.GetValue<Option, bool>(Option.HasTask);
		this.status = new PawnStatus(
			this.HasTask, loader.GetValue<Option, int>(Option.SeeQueenTaskRate));
	}

	private bool canSeeQueen(SingleRoleBase targetRole)
		=>
			this.status is not null &&
			targetRole.Id is ExtremeRoleId.Queen &&
			this.status.SeeQween;

}
