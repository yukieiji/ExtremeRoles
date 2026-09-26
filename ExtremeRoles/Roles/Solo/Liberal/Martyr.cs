using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.PRNG;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Martyr : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum MartyrOption
	{
		KillProbability,
		KillMoney,
		Range
	}

	public ExtremeAbilityButton Button { get; set; } = null!;

	public readonly record struct ExplosionOutcome(
		IReadOnlyList<byte> KilledTargets,
		float EarnedMoney);

	private int killProbability;
	private int killMoney;
	private float range;

	public Martyr() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.Martyr))
	{
	}

	public override string GetRoleTag()
		=> "Ma";

	public ExplosionOutcome ComputeExplosion(
		IReadOnlyList<PlayerControl> targetPcs, IRng rng)
	{
		var killed = new List<byte>();

		foreach (var target in targetPcs)
		{
			if (rng.Next(0, 100) < this.killProbability)
			{
				killed.Add(target.PlayerId);
			}
		}

		float earnedMoney = killed.Count * this.killMoney;
		return new ExplosionOutcome(killed, earnedMoney);
	}

	public void CreateAbility()
	{
		this.CreateNormalAbilityButton(
			"SelfDestruct",
			Resources.UnityObjectLoader.LoadFromResources<Sprite>(
				ObjectPath.Bomb));
		this.Button.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> IRoleAutoBuildAbility.IsCommonUse();

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public bool UseAbility()
	{
		byte selfId = PlayerControl.LocalPlayer.PlayerId;
		List<PlayerControl> targets = Player.GetAllPlayerInRange(
			PlayerControl.LocalPlayer, this, this.range);

		ExplosionOutcome outcome = this.ComputeExplosion(
			targets,
			RandomGenerator.Instance);

		foreach (byte targetId in outcome.KilledTargets)
		{
			Player.RpcUncheckMurderPlayer(
				selfId, targetId, byte.MinValue);
			ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
				targetId,
				Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Explosion);
		}

		// 自身は必ず死亡(KidsWinChecker前例)
		Player.RpcUncheckMurderPlayer(
			selfId, selfId, byte.MinValue);
		ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
			selfId,
			Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus.Explosion);

		if (outcome.EarnedMoney > 0f)
		{
			LiberalMoneyBankSystem.RpcUpdateSystem(
				selfId,
				LiberalMoneyHistory.Reason.MartyrSelfDestruct,
				outcome.EarnedMoney);
		}

		return true;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateCommonAbilityOption(factory);
		factory.CreateIntOption(
			MartyrOption.KillProbability,
			100, 10, 100, 10,
			format: OptionUnit.Percentage);
		factory.CreateIntOption(
			MartyrOption.KillMoney,
			10, 0, 100, 5);
		factory.CreateFloatOption(
			MartyrOption.Range,
			2.0f, 0.5f, 5.0f, 0.5f);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);

		this.killProbability = loader.GetValue<MartyrOption, int>(
			MartyrOption.KillProbability);
		this.killMoney = loader.GetValue<MartyrOption, int>(
			MartyrOption.KillMoney);
		this.range = loader.GetValue<MartyrOption, float>(
			MartyrOption.Range);
	}
}
