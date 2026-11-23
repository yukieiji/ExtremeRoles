using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;


using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Implemented;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

public sealed class FemmeFataleRole :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleSpecialReset,
	IRoleMurderPlayerHook,
	IRoleUpdate,
	IRoleMeetingButtonAbility,
	IRoleVoteModifier
{
	public enum FemmeFataleOption
	{
		CanUseVent,
		Range,
		JoseCandidatePromoteMeetingCount,
		JoseKillKillCoolReduceRate,
		JoseTaskKillCoolReduceRate,
		JoseTaskCompKillCoolReduceRate,
		JoseSelfKillTimer,
		VoteMultiplier,
		KillDefense,
		JoseCandidateKillDefense,
	}

	public enum RpcOps : byte
	{
		SpecialVote,
		CoverDead,
	}

	public int Order => (int)IRoleVoteModifier.ModOrder.FemmeFataleSpecialVote;

	public Sprite AbilityImage => UnityObjectLoader.LoadSpriteFromResources(
		ObjectPath.CaptainSpecialVote);

	public ExtremeAbilityButton? Button { get; set; }

	private float range;
	public float JoseSelfKillTimer { get; private set; }

	private PlayerControl? target;
	public override IStatusModel? Status => this.joseStatus;

	private FemmeFataleStatusModel? joseStatus;
	private FemmeFataleSpecialVoteHandler? specialVoteHandler;
	private FemmeFataleTaskCoolBoostHandler? taskCoolBoostHandler;
	private FemmeFataleKillCoolBoostHandler? killCoolBoostHandler;
	private FemmeFataleJosePromotionHandler? josePromotionHandler;

	public FemmeFataleRole() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.FemmeFatale,
			ColorPalette.FemmeFataleCrimson,
			RoleProp.CanKill | RolePropPresets.OptionalDefault))
	{
	}

	public static void HandleRpc(ref MessageReader reader)
	{
		RpcOps ops = (RpcOps)reader.ReadByte();
		switch (ops)
		{
			case RpcOps.SpecialVote:
			{
				byte femmeId = reader.ReadByte();
				byte targetId = reader.ReadByte();
				if (ExtremeRoleManager.TryGetSafeCastedRole<FemmeFataleRole>(femmeId, out var femme))
				{
					femme.SetTargetVote(targetId);
				}
				break;
			}
			case RpcOps.CoverDead:
			{
				byte killerPlayerId = reader.ReadByte();
				byte prevTargetPlayerId = reader.ReadByte();
				byte targetJosePlayerId = reader.ReadByte();
				FemmeFataleKillBlockHandler.CoverDead(
					killerPlayerId, prevTargetPlayerId, targetJosePlayerId);
				break;
			}
		}
	}

	public static void TargetToJose(byte rolePlayerId, byte targetPlayerId)
		=> FemmeFataleJosePromotionHandler.TargetToJose(rolePlayerId, targetPlayerId);

	public void AddJosePlayer(byte playerId)
		=> this.joseStatus?.AddJose(playerId);

	public void RemoveJosePlayer(byte playerId)
		=> this.joseStatus?.RemoveJose(playerId);

	public void HookMurderPlayer(
		PlayerControl source, PlayerControl target)
		=> this.killCoolBoostHandler?.HookMurderPlayer(source, target);

	public void Update(PlayerControl rolePlayer)
	{
		this.specialVoteHandler?.UpdateMeetingDisplay();
		this.taskCoolBoostHandler?.Update(rolePlayer);
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		removeAllJoses();
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"femmeCreateJose", UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.QueenCharm));
	}

	public bool UseAbility()
	{
		if (this.target == null ||
			this.josePromotionHandler is null)
		{
			return false;
		}

		this.josePromotionHandler.AddCandidate(this.target.PlayerId);

		return true;
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse() &&
			Player.TryGetClosestPlayerInRange(PlayerControl.LocalPlayer, this, range, out target) &&
			this.josePromotionHandler is not null &&
			this.josePromotionHandler.CanPromote(target);

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.josePromotionHandler?.UpdateMeetings();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		removeAllJoses();
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		removeAllJoses();
	}

	private void removeAllJoses()
	{
		this.josePromotionHandler?.RemoveAllJoses();
		this.taskCoolBoostHandler?.Reset();
	}

	public override string GetRolePlayerNameTag(
		SingleRoleBase targetRole, byte targetPlayerId)
		=> joseStatus is not null &&
			joseStatus.IsJoseOrCandidate(targetPlayerId) ? 
				Design.ColoredString(ColorPalette.FemmeFataleCrimson, $" ♡") : 
				base.GetRolePlayerNameTag(targetRole, targetPlayerId);

	public override bool IsSameTeam(SingleRoleBase targetRole)
		=> this.IsNeutralSameTeam(targetRole);

	public void SetTargetVote(byte targetPlayerId)
		=> this.specialVoteHandler?.SetTargetVote(targetPlayerId);

	public void ModifiedVote(
		byte rolePlayerId,
		ref Dictionary<byte, byte> voteTarget,
		ref Dictionary<byte, int> voteResult)
		=> this.specialVoteHandler?.ModifiedVote(ref voteTarget, ref voteResult);

	public IEnumerable<VoteInfo> GetModdedVoteInfo(
		VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
		=> this.specialVoteHandler is null ? [] : this.specialVoteHandler.GetModdedVoteInfo(collector, rolePlayer);

	public void ResetModifier()
		=> this.specialVoteHandler?.ResetModifier();

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> this.specialVoteHandler?.ButtonMod(instance, abilityButton);

	public Action CreateAbilityAction(PlayerVoteArea instance)
		=> this.specialVoteHandler is null ? () => { } : this.specialVoteHandler.CreateAbilityAction(instance);

	public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
		=> this.specialVoteHandler?.IsBlockMeetingButtonAbility(instance) ?? true;

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			FemmeFataleOption.CanUseVent,
			false);

		IRoleAbility.CreateAbilityCountOption(
			factory, 1, 2);

		factory.CreateFloatOption(
			FemmeFataleOption.Range,
			1.0f, 0.5f, 2.6f, 0.1f);
		factory.CreateIntOption(
			FemmeFataleOption.JoseCandidatePromoteMeetingCount,
			1, 1, 10, 1);

		factory.CreateIntOption(
			FemmeFataleOption.JoseKillKillCoolReduceRate,
			40, 0, 85, 1,
			format: OptionUnit.Percentage);
		factory.CreateIntOption(
			FemmeFataleOption.JoseTaskKillCoolReduceRate,
			75, 0, 99, 1,
			format: OptionUnit.Percentage);
		factory.CreateIntOption(
			FemmeFataleOption.JoseTaskCompKillCoolReduceRate,
			30, 0, 75, 1,
			format: OptionUnit.Percentage);

		factory.CreateFloatOption(
			FemmeFataleOption.JoseSelfKillTimer,
			45.0f, 10.0f, 120.0f, 1.0f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			FemmeFataleOption.VoteMultiplier,
			1.0f, 0.0f, 5.0f, 0.5f,
			format: OptionUnit.Multiplier);
		var defenseOpt = factory.CreateBoolOption(
			FemmeFataleOption.KillDefense,
			true);
		factory.CreateBoolOption(
			FemmeFataleOption.JoseCandidateKillDefense,
			true, new InvertActive(defenseOpt));
	}

	protected override void RoleSpecificInit()
	{
		var cate = Loader;

		range = cate.GetValue<FemmeFataleOption, float>(FemmeFataleOption.Range);
		UseVent = cate.GetValue<FemmeFataleOption, bool>(FemmeFataleOption.CanUseVent);
		JoseSelfKillTimer = cate.GetValue<FemmeFataleOption, float>(FemmeFataleOption.JoseSelfKillTimer);

		int killRate = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseKillKillCoolReduceRate);
		float killKillCoolReduceRate = killRate == 0 ? 1.0f : 1.0f - killRate / 100.0f;

		int taskRate = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseTaskKillCoolReduceRate);
		float taskKillCoolReduceRate = taskRate == 0 ? 1.0f : 1.0f - taskRate / 100.0f;

		int compRate = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseTaskCompKillCoolReduceRate);
		float taskCompKillCoolReduceRate = compRate == 0 ? 1.0f : 1.0f - compRate / 100.0f;

		float voteMultiplier = cate.GetValue<FemmeFataleOption, float>(FemmeFataleOption.VoteMultiplier);
		bool killDefense = cate.GetValue<FemmeFataleOption, bool>(FemmeFataleOption.KillDefense);
		bool candidateKillDefense = cate.GetValue<FemmeFataleOption, bool>(FemmeFataleOption.JoseCandidateKillDefense);
		int promoteMeetingCount = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseCandidatePromoteMeetingCount);

		this.joseStatus = new FemmeFataleStatusModel();
		this.specialVoteHandler = new FemmeFataleSpecialVoteHandler(this.joseStatus, voteMultiplier);
		this.taskCoolBoostHandler = new FemmeFataleTaskCoolBoostHandler(this, this.joseStatus, taskKillCoolReduceRate, taskCompKillCoolReduceRate);
		this.killCoolBoostHandler = new FemmeFataleKillCoolBoostHandler(this.joseStatus, killKillCoolReduceRate);
		this.josePromotionHandler = new FemmeFataleJosePromotionHandler(this.joseStatus, promoteMeetingCount);
		this.AbilityClass = new FemmeFataleKillBlockHandler(this.joseStatus, killDefense, killDefense && candidateKillDefense);
	}
}
