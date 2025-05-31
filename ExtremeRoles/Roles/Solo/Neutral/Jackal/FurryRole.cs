using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.RoleAssign;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public sealed class FurryRole : SingleRoleBase, IRoleWinPlayerModifier, IRoleUpdate
{
	public enum Option
	{
		UseVent,
		HasTask,
		SeeJackalTaskRate,
	}

	public override IStatusModel? Status => this.status;

	private FurryStatus? status;
	private bool isUpdate;

	public FurryRole() : base(
		ExtremeRoleId.Furry,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Furry.ToString(),
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

	public static void BecomeToJackal(byte targetJackal, byte targetFurry)
	{
		var curJackal = ExtremeRoleManager.GetSafeCastedRole<JackalRole>(targetJackal);
		if (curJackal == null) { return; }
		var newJackal = (JackalRole)curJackal.Clone();

		newJackal.Initialize();
		if (targetFurry == PlayerControl.LocalPlayer.PlayerId)
		{
			newJackal.CreateAbility();
		}

		if (newJackal.Button?.Behavior is ICountBehavior countBehavior)
		{
			countBehavior.SetAbilityCount(0);
		}

		newJackal.CurRecursion = 0;
		newJackal.SidekickPlayerId = [];
		newJackal.SetControlId(curJackal.GameControlId);

		ExtremeRoleManager.SetNewRole(targetFurry, newJackal);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
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
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.HasTask = loader.GetValue<Option, bool>(Option.HasTask);
		this.status = new FurryStatus(
			this.HasTask,
			loader.GetValue<Option, int>(Option.SeeJackalTaskRate));
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (ShipStatus.Instance == null ||
			!ShipStatus.Instance.enabled ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			this.isUpdate ||
			this.status is null)
		{
			return;
		}

		this.status.Update(rolePlayer);

		if (this.status.TargetJackal.HasValue)
		{

			ExtremeRoleManager.RpcReplaceRole(
				this.status.TargetJackal.Value, rolePlayer.PlayerId,
				ExtremeRoleManager.ReplaceOperation.RebornJackal);
			this.isUpdate = true;
		}
	}
	private bool canSeeJackal(SingleRoleBase targetRole)
		=>
			this.status is not null &&
			targetRole.Id is ExtremeRoleId.Jackal &&
			this.status.SeeJackal;
}
