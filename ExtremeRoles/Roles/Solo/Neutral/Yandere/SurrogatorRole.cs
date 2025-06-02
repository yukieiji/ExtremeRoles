using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;

#nullable enable

public sealed class SurrogatorRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleWinPlayerModifier
{
	public enum Option
	{
		UseVent,
		HasTask,
		SeeYandereTaskRate,
		Range,
		PreventNum,
		PreventKillTime,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private NetworkedPlayerInfo? targetBody;
	private NetworkedPlayerInfo DeadBody => Player.GetDeadBodyInfo(this.range);
	private byte activateTarget;
	private float range;

	public override IStatusModel? Status => this.status;
	private SurrogatorStatus? status;

	public SurrogatorRole() : base(
		ExtremeRoleId.Surrogator,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Surrogator.ToString(),
		ColorPalette.YandereVioletRed,
		false, false, false, false)
	{ }

	public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)
		=> canSeeYandere(targetRole) ?
				ColorPalette.YandereVioletRed :
				base.GetTargetRoleSeeColor(targetRole, targetPlayerId);

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

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"死体纏い",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.EvolverEvolved),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ForceCleanUp);
	}

	public bool IsAbilityUse()
	{
		this.targetBody = this.DeadBody;
		return IRoleAbility.IsCommonUse() && this.targetBody != null;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
	}

	public bool UseAbility()
	{
		if (this.targetBody == null)
		{
			return false;
		}
		this.activateTarget = this.targetBody.PlayerId;
		return true;
	}

	public void CleanUp()
	{
		Player.RpcCleanDeadBody(this.activateTarget);

		SurrogatorGurdSystem.RpcReduce();

		this.activateTarget = byte.MaxValue;
	}

	public bool CheckAbility()
	{
		var check = this.DeadBody;
		return
			check != null &&
			this.activateTarget == check.PlayerId;
	}

	public void ForceCleanUp()
	{
		this.targetBody = null;
	}

	public static bool TryGurdOnesideLover(PlayerControl killer, byte targetPlayerId)
	{
		if (!(
				ExtremeSystemTypeManager.Instance.TryGet<SurrogatorGurdSystem>(
					ExtremeSystemType.SurrogatorGurdSystem, out var system) &&
				system.CanGuard(targetPlayerId)
			))
		{
			return false;
		}

		killer.SetKillTimer(system.PreventKillTime);

		SurrogatorGurdSystem.RpcReduce();
		return true;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(Option.UseVent, false);
		var taskOpt = factory.CreateBoolOption(
			Option.HasTask, false);
		factory.CreateIntOption(
			Option.SeeYandereTaskRate, 50, 0, 100, 10,
			taskOpt, format: OptionUnit.Percentage);
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10, 3.0f);
		factory.CreateFloatOption(Option.Range, 0.7f, 0.1f, 3.5f, 0.1f);
		factory.CreateIntOption(Option.PreventNum, 1, 0, 10, 1);
		factory.CreateFloatOption(
			Option.PreventKillTime, 20.0f, 2.5f, 30.0f, 0.5f, format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.range = loader.GetValue<Option, float>(Option.Range);
		var system = SurrogatorGurdSystem.CreateOrGet(
			loader.GetValue<Option, float>(Option.PreventKillTime));
		system.AddGuardNum(
			loader.GetValue<Option, int>(Option.PreventNum));

		this.status = new SurrogatorStatus(
			this.HasTask,
			loader.GetValue<Option, int>(Option.SeeYandereTaskRate));
	}

	private bool canSeeYandere(SingleRoleBase targetRole)
		=>
			this.status is not null &&
			targetRole.Id is ExtremeRoleId.Yandere &&
			this.status.SeeYandere;
}
