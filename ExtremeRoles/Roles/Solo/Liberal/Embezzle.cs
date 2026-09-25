using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Embezzle : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
	public enum EmbezzleOption
	{
		CanSeeTaskBar,
		AllowMultipleTargetsPerPhase,
		NoTaskMoneyGuarantee,
		MaxDisguiseTaskNum,
		Range,
		TaskCompletedMoney
	}

	public bool CanSeeTaskBar { get; private set; }
	public ExtremeAbilityButton Button { get; set; } = null!;

	private readonly HashSet<byte> targetedPlayers = new();
	private bool allowMultipleTargets;
	private bool noTaskMoneyGuarantee;
	private int maxDisguiseTaskNum;
	private float range;
	private int taskCompletedMoney;
	private byte currentTarget = byte.MaxValue;

	public Embezzle() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Embezzle))
	{
	}

	public Embezzle(LiberalDefaultOptionLoader option) : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Embezzle))
	{
		LiberalSettingOverrider.OverrideDefault(this, option);
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		string baseName = base.GetRolePlayerNameTag(targetRole, targetPlayerId);
		if (this.targetedPlayers.Contains(targetPlayerId))
		{
			return baseName + Design.ColoredString(this.Core.Color, " ★");
		}
		return baseName;
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"DisguisedAccounting",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.AgencyTakeTask));
		this.Button.SetLabelToCrewmate();
	}

	public bool UseAbility()
	{
		if (!this.allowMultipleTargets && this.targetedPlayers.Count > 0)
		{
			return false;
		}

		if (this.currentTarget != byte.MaxValue)
		{
			this.targetedPlayers.Add(this.currentTarget);
			this.currentTarget = byte.MaxValue;
			return true;
		}
		return false;
	}

	public bool IsAbilityUse()
	{
		this.currentTarget = byte.MaxValue;

		if (!this.allowMultipleTargets && this.targetedPlayers.Count > 0)
		{
			return false;
		}

		PlayerControl target = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this, this.range);

		if (target != null)
		{
			this.currentTarget = target.PlayerId;
		}

		return IRoleAutoBuildAbility.IsCommonUse() &&
			this.currentTarget != byte.MaxValue &&
			!this.targetedPlayers.Contains(this.currentTarget);
	}

	public void ResetOnMeetingStart()
	{
		float totalMoneyGained = 0f;

		foreach (byte targetId in this.targetedPlayers)
		{
			NetworkedPlayerInfo? targetPlayerInfo = GameData.Instance.GetPlayerById(targetId);
			if (targetPlayerInfo == null)
			{
				continue;
			}

			int completedCount = 0;
			if (targetPlayerInfo.Tasks != null && targetPlayerInfo.Tasks.Count > 0)
			{
				foreach (var task in targetPlayerInfo.Tasks)
				{
					if (completedCount >= this.maxDisguiseTaskNum)
					{
						break;
					}

					if (!task.Complete)
					{
						completedCount++;
					}
				}
			}

			float moneyForTarget = completedCount * this.taskCompletedMoney;

			if (completedCount == 0 && this.noTaskMoneyGuarantee)
			{
				moneyForTarget = this.taskCompletedMoney;
			}

			totalMoneyGained += moneyForTarget;
		}

		if (totalMoneyGained > 0f)
		{
			LiberalMoneyBankSystem.RpcUpdateSystem(
				PlayerControl.LocalPlayer.PlayerId,
				Module.GameResult.LiberalMoneyHistory.Reason.AddOnTask,
				totalMoneyGained);
		}

		this.targetedPlayers.Clear();
		this.currentTarget = byte.MaxValue;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			EmbezzleOption.CanSeeTaskBar,
			true);
		factory.CreateBoolOption(
			EmbezzleOption.AllowMultipleTargetsPerPhase,
			false);
		factory.CreateBoolOption(
			EmbezzleOption.NoTaskMoneyGuarantee,
			true);
		factory.CreateIntOption(
			EmbezzleOption.MaxDisguiseTaskNum,
			2, 1, 5, 1);
		factory.CreateFloatOption(
			EmbezzleOption.Range,
			1.0f, 0.5f, 3.5f, 0.25f);
		factory.CreateIntOption(
			EmbezzleOption.TaskCompletedMoney,
			5, 1, 100, 1);

		IRoleAbility.CreateAbilityCountOption(
			factory, 2, 5);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.CanSeeTaskBar = loader.GetValue<EmbezzleOption, bool>(
			EmbezzleOption.CanSeeTaskBar);
		this.allowMultipleTargets = loader.GetValue<EmbezzleOption, bool>(
			EmbezzleOption.AllowMultipleTargetsPerPhase);
		this.noTaskMoneyGuarantee = loader.GetValue<EmbezzleOption, bool>(
			EmbezzleOption.NoTaskMoneyGuarantee);
		this.maxDisguiseTaskNum = loader.GetValue<EmbezzleOption, int>(
			EmbezzleOption.MaxDisguiseTaskNum);
		this.range = loader.GetValue<EmbezzleOption, float>(
			EmbezzleOption.Range);
		this.taskCompletedMoney = loader.GetValue<EmbezzleOption, int>(
			EmbezzleOption.TaskCompletedMoney);
	}
}
