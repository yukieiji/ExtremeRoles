using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory.Old;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class KnightRole : SingleRoleBase, IRoleWinPlayerModifier, IRoleUpdate
{
	public enum Option
	{
		IsSubTeam,
		UseVent,
		HasTask,
		SeeQueenTaskRate,
		CanKillQueen,
		CanKillServant,
	}

	public override IStatusModel? Status => this.status;

	private KnightStatus? status;

	private bool canNotKillQueen;
	private bool canNotKillServant;

	public KnightRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Knight,
			ColorPalette.QueenWhite),
		true, false, false, false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case (GameOverReason)RoleGameOverReason.QueenKillAllOther:
				winner.AddWithPlus(rolePlayerInfo);
				break;
			default:
				break;
		}
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.Update(rolePlayer);
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
	{
		var id = targetRole.Core.Id;
		if (this.canSeeQueen(targetRole) && (
			(
				this.canNotKillQueen &&
				id is ExtremeRoleId.Queen
			)
			||
			(
				this.canNotKillServant &&
				id is ExtremeRoleId.Servant ||
				(
					targetRole is MultiAssignRoleBase multiRole &&
					multiRole.AnotherRole != null &&
					multiRole.AnotherRole.Core.Id is ExtremeRoleId.Servant
				)
			)))
		{
			return true;
		}

		if (id == this.Core.Id)
		{
			if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
			{
				return true;
			}
			else
			{
				return IsSameControlId(targetRole);
			}
		}
		else
		{
			return base.IsSameTeam(targetRole);
		}
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeQueen(targetRole) ?
				Design.ColoredString(
					ColorPalette.QueenWhite,
					$" {QueenRole.RoleShowTag}") :
				base.GetRolePlayerNameTag(targetRole, targetPlayerId);

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeQueen(targetRole) ?
				ColorPalette.QueenWhite :
				base.GetTargetRoleSeeColor(targetRole, targetPlayerId);

	protected override void CreateSpecificOption(OldAutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			Option.IsSubTeam, true);
		factory.CreateBoolOption(
			Option.UseVent, false);
		var taskOpt = factory.CreateBoolOption(
			Option.HasTask, false);
		factory.CreateIntOption(
			Option.SeeQueenTaskRate, 50, 0, 100, 10,
			taskOpt, format: OptionUnit.Percentage);

		factory.CreateBoolOption(Option.CanKillQueen, true, taskOpt);
		factory.CreateBoolOption(Option.CanKillServant, true, taskOpt);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.HasTask = loader.GetValue<Option, bool>(Option.HasTask);

		this.canNotKillQueen = !loader.GetValue<Option, bool>(Option.CanKillQueen);
		this.canNotKillServant = !loader.GetValue<Option, bool>(Option.CanKillServant);

		this.status = new KnightStatus(
			this.HasTask,
			loader.GetValue<Option, int>(Option.SeeQueenTaskRate),
			this.CanKill && loader.GetValue<Option, bool>(Option.IsSubTeam));
	}

	private bool canSeeQueen(SingleRoleBase targetRole)
		=>
			this.status is not null &&
			targetRole.Core.Id is ExtremeRoleId.Queen &&
			this.status.SeeQween;

}
