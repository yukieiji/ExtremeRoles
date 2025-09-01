using UnityEngine;

using ExtremeRoles.GameMode;
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
		CanKillYandere,
		CanKillOneSideLover,
	}

	public override IStatusModel? Status => this.status;

	private IntimateStatus? status;

	private bool canNotKillYandere;
	private bool canNotKillOneSideLover;

	public IntimateRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Intimate,
			ColorPalette.YandereVioletRed),
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
		if (this.canSeeYandere(targetRole) && (
			(
				this.canNotKillYandere &&
				id is ExtremeRoleId.Yandere
			)
			||
			(
				this.canNotKillOneSideLover &&
				this.status is not null &&
				this.status.IsOneSideLover(targetRole)
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


	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeYandere(targetRole) ?
				ColorPalette.YandereVioletRed :
				base.GetTargetRoleSeeColor(targetRole, targetPlayerId);

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		var killOpt = factory.CreateBoolOption(
			Option.CanKill, true);
		CreateKillerOption(factory, killOpt, true, true);
		factory.CreateBoolOption(
			Option.IsSubTeam, true, killOpt, invert: true);
		factory.CreateBoolOption(
			Option.UseVent, false);
		var taskOpt = factory.CreateBoolOption(
			Option.HasTask, false);
		factory.CreateIntOption(
			Option.SeeYandereTaskRate, 50, 0, 100, 10,
			taskOpt, format: OptionUnit.Percentage);

		factory.CreateBoolOption(Option.CanKillYandere, true, taskOpt);
		factory.CreateBoolOption(Option.CanKillOneSideLover, true, taskOpt);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.CanKill = loader.GetValue<Option, bool>(Option.CanKill);
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.HasTask = loader.GetValue<Option, bool>(Option.HasTask);

		this.canNotKillYandere = !loader.GetValue<Option, bool>(Option.CanKillYandere);
		this.canNotKillOneSideLover = !loader.GetValue<Option, bool>(Option.CanKillOneSideLover);

		this.status = new IntimateStatus(
			this.HasTask,
			loader.GetValue<Option, int>(Option.SeeYandereTaskRate),
			this.CanKill && loader.GetValue<Option, bool>(Option.IsSubTeam));
	}

	private bool canSeeYandere(SingleRoleBase targetRole)
			=>
				this.status is not null &&
				targetRole.Core.Id is ExtremeRoleId.Yandere &&
				this.status.SeeYandere;

}
