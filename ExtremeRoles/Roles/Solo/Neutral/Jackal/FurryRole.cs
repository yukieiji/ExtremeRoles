using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.GameResult;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomOption.Factory.Old;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public sealed class FurryRole : SingleRoleBase,
	IRoleWinPlayerModifier,
	IRoleUpdate,
	IRoleMurderPlayerHook
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
		RoleCore.BuildNeutral(
			ExtremeRoleId.Furry,
			ColorPalette.JackalBlue),
		false, false, false, false)
	{ }

	public void HookMuderPlayer(PlayerControl _, PlayerControl target)
	{
		if (this.isUpdate ||
			target == null ||
			!ExtremeRoleManager.TryGetSafeCastedRole<JackalRole>(target.PlayerId, out var jackalRole) ||
			target.Data == null)
		{
			return;
		}

		bool allSidekicksDead = true;
		foreach (byte sidekickPlayerId in jackalRole.SidekickPlayerId)
		{
			var sidekickPlayer = GameData.Instance.GetPlayerById(sidekickPlayerId);
			if (sidekickPlayer != null && !(sidekickPlayer.IsDead || sidekickPlayer.Disconnected))
			{
				allSidekicksDead = false;
				break;
			}
		}

		if (!allSidekicksDead)
		{
			return;
		}

		var local = PlayerControl.LocalPlayer;
		// SKを残したままJackal昇格している可能性を確認 == 同じコントロールIDでJackalがいる
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				player.Data.IsDead ||
				player.Data.Disconnected ||
				player.PlayerId == target.PlayerId ||
				player.PlayerId == local.PlayerId ||
				!ExtremeRoleManager.TryGetSafeCastedRole<JackalRole>(player.PlayerId, out var checkJk) ||
				checkJk.GameControlId != jackalRole.GameControlId)
			{
				continue;
			}
			return;
		}
		ExtremeRoleManager.RpcReplaceRole(
			target.PlayerId, PlayerControl.LocalPlayer.PlayerId,
		ExtremeRoleManager.ReplaceOperation.RebornJackal);
		this.isUpdate = true;
	}

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case (GameOverReason)RoleGameOverReason.JackalKillAllOther:
				winner.AddWithPlus(rolePlayerInfo);
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

	protected override void CreateSpecificOption(OldAutoParentSetOptionCategoryFactory factory)
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
		this.status?.Update(rolePlayer);
	}
	private bool canSeeJackal(SingleRoleBase targetRole)
		=>
			this.status is not null &&
			targetRole.Core.Id is ExtremeRoleId.Jackal &&
			this.status.SeeJackal;
}
