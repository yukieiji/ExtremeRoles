using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;
using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

public sealed class FemmeFataleRole :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleSpecialReset,
	IRoleMurderPlayerHook,
	IRoleUpdate,
	IRoleResetMeeting,
	IRoleMeetingButtonAbility,
	IRoleVoteModifier
{
	public const string RoleShowTag = "◯j";

	public enum FemmeFataleOption
	{
		Range,
		CanUseVent,
		JoseKillKillCoolReduceRate,
		JoseTaskKillCoolReduceRate,
		JoseTaskCompKillCoolReduceRate,
		JoseSelfKillTimer,
		VoteMultiplier,
		KillDefense,
	}

	public enum RpcOps : byte
	{
		MarkJose,
		SpecialVote,
		CoverDead,
	}

	public int Order => (int)IRoleVoteModifier.ModOrder.FemmeFataleSpecialVote;

	public Sprite AbilityImage => UnityObjectLoader.LoadSpriteFromResources(
		ObjectPath.CaptainSpecialVote);

	public ExtremeAbilityButton? Button
	{
		get => createJose;
		set => createJose = value;
	}

	public PlayerControl? Target;
	private ExtremeAbilityButton? createJose;
	private float range;
	private float killKillCoolReduceRate;
	private float taskKillCoolReduceRate;
	private float taskCompKillCoolReduceRate;
	public float JoseSelfKillTimer { get; private set; }
	private float voteMultiplier;
	private bool killDefense;

	private Dictionary<byte, float> joseTaskGage = new();
	private HashSet<byte> taskCompJose = new();
	private HashSet<byte> josePlayerId = new();
	private HashSet<byte> pendingJosePlayerId = new();

	private byte voteTarget = byte.MaxValue;
	private Dictionary<byte, SpriteRenderer> voteCheckMark = new();
	private TMPro.TextMeshPro? meetingVoteText;

	public FemmeFataleRole() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.FemmeFatale,
			ColorPalette.FemmeFataleCrimson,
			RoleProp.CanKill | RolePropPresets.OptionalDefault))
	{ }

	public static void HandleRpc(ref MessageReader reader)
	{
		RpcOps ops = (RpcOps)reader.ReadByte();
		switch (ops)
		{
			case RpcOps.MarkJose:
			{
				byte femmeId = reader.ReadByte();
				byte targetId = reader.ReadByte();
				if (ExtremeRoleManager.TryGetSafeCastedRole<FemmeFataleRole>(femmeId, out var femme))
				{
					femme.pendingJosePlayerId.Add(targetId);
				}
				break;
			}
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
				coverDead(killerPlayerId, prevTargetPlayerId, targetJosePlayerId);
				break;
			}
		}
	}

	public static bool TryRpcKillGuardedJose(byte killerPlayerId, byte targetPlayerId)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedRole<FemmeFataleRole>(targetPlayerId, out var femme))
		{
			return false;
		}

		if (!femme.killDefense)
		{
			return false;
		}

		List<byte> livingJoses = new();
		foreach (byte jId in femme.josePlayerId)
		{
			var p = Player.GetPlayerControlById(jId);
			if (p != null && p.IsAlive())
			{
				livingJoses.Add(jId);
			}
		}

		if (livingJoses.Count == 0)
		{
			return false;
		}

		int idx = RandomGenerator.Instance.Next(0, livingJoses.Count);
		byte chosenJose = livingJoses[idx];

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FemmeFataleAbility))
		{
			caller.WriteByte((byte)RpcOps.CoverDead);
			caller.WriteByte(killerPlayerId);
			caller.WriteByte(targetPlayerId);
			caller.WriteByte(chosenJose);
		}

		coverDead(killerPlayerId, targetPlayerId, chosenJose);
		return true;
	}

	private static void coverDead(byte killerPlayerId, byte prevTargetPlayerId, byte targetJosePlayerId)
	{
		byte localId = PlayerControl.LocalPlayer.PlayerId;
		if (localId == targetJosePlayerId || localId == prevTargetPlayerId)
		{
			Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.6f);
		}

		RPCOperator.UncheckedMurderPlayer(killerPlayerId, targetJosePlayerId, byte.MinValue);

		if (!Player.TryGetPlayerControl(targetJosePlayerId, out var josePlayer) ||
			josePlayer.Data == null ||
			!josePlayer.Data.IsDead ||
			josePlayer.Data.Disconnected)
		{
			return;
		}

		ExtremeRolesPlugin.ShipState.ReplaceDeadReason(
			targetJosePlayerId, ExtremeShipStatus.PlayerStatus.Martyrdom);
	}

	public static void TargetToJose(byte rolePlayerId, byte targetPlayerId)
	{
		FemmeFataleRole? femme = ExtremeRoleManager.GetSafeCastedRole<FemmeFataleRole>(rolePlayerId);
		if (femme == null) { return; }

		var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (targetPlayer == null) { return; }
		var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

		IParentChainStatus.PurgeParent(targetPlayerId);
		resetTargetAnotherRole(targetRole, targetPlayerId, targetPlayer);
		replaceVanilaRole(targetRole, targetPlayer);
		resetAbility(targetRole, targetPlayerId);

		JoseRole jose = new JoseRole(
			rolePlayerId, femme, targetRole, femme.JoseSelfKillTimer);

		var core = targetRole.Core;
		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			if (core.Team != ExtremeRoleType.Neutral)
			{
				if (jose.Button != null)
				{
					jose.Button.HotKey = KeyCode.C;
				}
			}
			HudManager.Instance.ReGridButtons();
		}

		if (core.Team != ExtremeRoleType.Neutral)
		{
			core.Team = ExtremeRoleType.Neutral;

			if (targetRole is VanillaRoleWrapper vanillaRole)
			{
				vanillaRole.AnotherRole = null;
				vanillaRole.CanHasAnotherRole = false;
				vanillaRole.CanCallMeeting = true;
				vanillaRole.CanUseAdmin = true;
				vanillaRole.CanUseSecurity = true;
				vanillaRole.CanUseVital = true;

				jose.CanHasAnotherRole = true;
				jose.SetAnotherRole(vanillaRole);

				ExtremeRoleManager.SetNewRole(targetPlayerId, jose);
			}
			else if (targetRole is MultiAssignRoleBase multiAssignRole)
			{
				multiAssignRole.AnotherRole = null;

				multiAssignRole.CanHasAnotherRole = true;
				jose.CanHasAnotherRole = false;

				ExtremeRoleManager.SetNewAnothorRole(targetPlayerId, jose);
			}
			else
			{
				jose.CanHasAnotherRole = true;
				jose.SetAnotherRole(targetRole);

				ExtremeRoleManager.SetNewRole(targetPlayerId, jose);
			}
		}
		else
		{
			jose.CanHasAnotherRole = false;
			resetRole(targetRole, targetPlayerId, targetPlayer);
			ExtremeRoleManager.SetNewRole(targetPlayerId, jose);
		}

		femme.AddJosePlayer(targetPlayerId);
	}

	public void AddJosePlayer(byte playerId)
	{
		this.josePlayerId.Add(playerId);
	}

	public void RemoveJosePlayer(byte playerId)
	{
		this.josePlayerId.Remove(playerId);
	}

	public int GetLivingJoseCount()
	{
		int count = 0;
		foreach (byte id in this.josePlayerId)
		{
			var p = Player.GetPlayerControlById(id);
			if (p != null && p.IsAlive())
			{
				count++;
			}
		}
		return count;
	}

	public void HookMurderPlayer(
		PlayerControl source, PlayerControl target)
	{
		if (source.PlayerId != target.PlayerId &&
			josePlayerId.Contains(source.PlayerId))
		{
			float killcool = PlayerControl.LocalPlayer.killTimer;
			if (killcool > 0.0f && killKillCoolReduceRate < 1.0f)
			{
				PlayerControl.LocalPlayer.killTimer = killcool * killKillCoolReduceRate;
			}
		}
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (MeetingHud.Instance != null && rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			if (meetingVoteText == null)
			{
				meetingVoteText = UnityEngine.Object.Instantiate(
					HudManager.Instance.TaskPanel.taskText,
					MeetingHud.Instance.transform);
				meetingVoteText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
				meetingVoteText.transform.position = Vector3.zero;
				meetingVoteText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
				meetingVoteText.transform.localScale *= 0.9f;
				meetingVoteText.color = Palette.White;
				meetingVoteText.gameObject.SetActive(false);
			}

			int livingJose = GetLivingJoseCount();
			int addVote = (int)Math.Floor(livingJose * this.voteMultiplier);
			meetingVoteText.text = Tr.GetString(
				"captainVoteStatus",
				addVote < 1 ? Tr.GetString("cannotDo") : Tr.GetString("canDo"),
				addVote);
			meetingVoteText.gameObject.SetActive(true);
		}

		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		foreach (byte playerId in josePlayerId)
		{
			var player = Player.GetPlayerControlById(playerId);
			if (player == null || !player) { continue; }

			float gage = Player.GetPlayerTaskGage(player);
			if (!joseTaskGage.ContainsKey(playerId))
			{
				joseTaskGage.Add(playerId, 0.0f);
			}
			float prevGage = joseTaskGage[playerId];
			joseTaskGage[playerId] = gage;

			float killcool = PlayerControl.LocalPlayer.killTimer;
			if (gage > prevGage && killcool > 0.0f && taskKillCoolReduceRate < 1.0f)
			{
				PlayerControl.LocalPlayer.killTimer = killcool * taskKillCoolReduceRate;
			}
			if (gage >= 1.0f && !taskCompJose.Contains(playerId))
			{
				taskCompJose.Add(playerId);
				if (taskCompKillCoolReduceRate < 1.0f)
				{
					if (!HasOtherKillCool)
					{
						KillCoolTime = Player.DefaultKillCoolTime;
					}
					HasOtherKillCool = true;
					KillCoolTime = KillCoolTime * taskCompKillCoolReduceRate;
				}
			}
		}
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
		if (this.Target == null)
		{
			return false;
		}

		byte targetId = this.Target.PlayerId;
		byte localId = PlayerControl.LocalPlayer.PlayerId;

		this.pendingJosePlayerId.Add(targetId);

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FemmeFataleAbility))
		{
			caller.WriteByte((byte)RpcOps.MarkJose);
			caller.WriteByte(localId);
			caller.WriteByte(targetId);
		}

		return true;
	}

	public bool IsAbilityUse()
	{
		Target = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer,
			this, range);

		if (Target != null && (josePlayerId.Contains(Target.PlayerId) || pendingJosePlayerId.Contains(Target.PlayerId)))
		{
			Target = null;
		}

		return Target != null && IRoleAbility.IsCommonUse();
	}

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		if (this.pendingJosePlayerId.Count > 0)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;
			foreach (byte targetId in this.pendingJosePlayerId.ToArray())
			{
				ExtremeRoleManager.RpcReplaceRole(
					localId, targetId, ExtremeRoleManager.ReplaceOperation.CreateJose);
			}
			this.pendingJosePlayerId.Clear();
		}
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
		byte localId = PlayerControl.LocalPlayer.PlayerId;
		foreach (byte playerId in josePlayerId.ToArray())
		{
			ExtremeRoleManager.RpcReplaceRole(
				localId, playerId, ExtremeRoleManager.ReplaceOperation.RemoveJose);
		}
		josePlayerId.Clear();
		pendingJosePlayerId.Clear();
	}

	public override Color GetTargetRoleSeeColor(
		SingleRoleBase targetRole,
		byte targetPlayerId)
	{
		if (josePlayerId.Contains(targetPlayerId) || pendingJosePlayerId.Contains(targetPlayerId))
		{
			return ColorPalette.FemmeFataleCrimson;
		}

		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public override string GetRoleTag() => RoleShowTag;

	public override string GetRolePlayerNameTag(
		SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (josePlayerId.Contains(targetPlayerId) || pendingJosePlayerId.Contains(targetPlayerId))
		{
			return Design.ColoredString(
				ColorPalette.FemmeFataleCrimson,
				$" {RoleShowTag}");
		}

		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
	{
		if (targetRole.Core.Id == Core.Id)
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

	public void SetTargetVote(byte targetPlayerId)
	{
		this.voteTarget = targetPlayerId;
	}

	public void ModifiedVote(
		byte rolePlayerId,
		ref Dictionary<byte, byte> voteTarget,
		ref Dictionary<byte, int> voteResult)
	{
		if (this.voteTarget == byte.MaxValue)
		{
			return;
		}

		int livingJose = GetLivingJoseCount();
		int addVoteNum = (int)Math.Floor(livingJose * this.voteMultiplier);

		if (addVoteNum <= 0)
		{
			return;
		}

		if (voteResult.TryGetValue(this.voteTarget, out int curVoteNum))
		{
			voteResult[this.voteTarget] = curVoteNum + addVoteNum;
		}
		else
		{
			voteResult[this.voteTarget] = addVoteNum;
		}
	}

	public IEnumerable<VoteInfo> GetModdedVoteInfo(
		VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
	{
		if (this.voteTarget == byte.MaxValue)
		{
			yield break;
		}

		int livingJose = GetLivingJoseCount();
		int addVoteNum = (int)Math.Floor(livingJose * this.voteMultiplier);
		if (addVoteNum > 0)
		{
			yield return new VoteInfo(rolePlayer.PlayerId, this.voteTarget, addVoteNum);
		}
	}

	public void ResetModifier()
	{
		this.voteTarget = byte.MaxValue;
		this.voteCheckMark.Clear();
	}

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "captainSpecialVote");

	public Action CreateAbilityAction(PlayerVoteArea instance)
	{
		void setTarget()
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FemmeFataleAbility))
			{
				caller.WriteByte((byte)RpcOps.SpecialVote);
				caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
				caller.WriteByte(instance.PlayerId);
			}
			this.SetTargetVote(instance.PlayerId);

			foreach (SpriteRenderer vote in this.voteCheckMark.Values)
			{
				if (vote != null)
				{
					vote.gameObject.SetActive(false);
				}
			}

			if (!this.voteCheckMark.TryGetValue(
					instance.PlayerId,
					out var checkMark) ||
				checkMark == null)
			{
				checkMark = UnityEngine.Object.Instantiate(
					instance.Background, instance.LevelNumberText.transform);
				checkMark.name = $"femmeFatale_SpecialVoteCheckMark_{instance.PlayerId}";
				checkMark.sprite = UnityObjectLoader.LoadSpriteFromResources(
					ObjectPath.CaptainSpecialVoteCheck);
				checkMark.transform.localPosition = new Vector3(7.25f, -0.5f, -3f);
				checkMark.transform.localScale = new Vector3(1.0f, 3.5f, 1.0f);
				checkMark.gameObject.layer = 5;
				this.voteCheckMark[instance.PlayerId] = checkMark;
			}
			checkMark.gameObject.SetActive(true);
		}

		return setTarget;
	}

	public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
	{
		int livingJose = GetLivingJoseCount();
		int addVote = (int)Math.Floor(livingJose * this.voteMultiplier);
		return addVote < 1;
	}

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
		factory.CreateBoolOption(
			FemmeFataleOption.KillDefense,
			true);
	}

	protected override void RoleSpecificInit()
	{
		var cate = Loader;

		range = cate.GetValue<FemmeFataleOption, float>(FemmeFataleOption.Range);
		UseVent = cate.GetValue<FemmeFataleOption, bool>(FemmeFataleOption.CanUseVent);
		JoseSelfKillTimer = cate.GetValue<FemmeFataleOption, float>(FemmeFataleOption.JoseSelfKillTimer);

		int killRate = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseKillKillCoolReduceRate);
		killKillCoolReduceRate = killRate == 0 ? 1.0f : 1.0f - killRate / 100.0f;

		int taskRate = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseTaskKillCoolReduceRate);
		taskKillCoolReduceRate = taskRate == 0 ? 1.0f : 1.0f - taskRate / 100.0f;

		int compRate = cate.GetValue<FemmeFataleOption, int>(FemmeFataleOption.JoseTaskCompKillCoolReduceRate);
		taskCompKillCoolReduceRate = compRate == 0 ? 1.0f : 1.0f - compRate / 100.0f;

		voteMultiplier = cate.GetValue<FemmeFataleOption, float>(FemmeFataleOption.VoteMultiplier);
		killDefense = cate.GetValue<FemmeFataleOption, bool>(FemmeFataleOption.KillDefense);

		joseTaskGage = new Dictionary<byte, float>();
		josePlayerId = new HashSet<byte>();
		pendingJosePlayerId = new HashSet<byte>();
		taskCompJose = new HashSet<byte>();
		voteCheckMark = new Dictionary<byte, SpriteRenderer>();
	}

	private static void resetTargetAnotherRole(
		SingleRoleBase targetRole,
		byte targetPlayerId,
		PlayerControl targetPlayer)
	{
		var multiAssignRole = targetRole as MultiAssignRoleBase;
		if (multiAssignRole == null) { return; }

		if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
		{
			if (multiAssignRole.AnotherRole is IRoleAbility abilityRole)
			{
				abilityRole.Button.OnMeetingStart();
			}
			if (multiAssignRole.AnotherRole is IRoleResetMeeting meetingResetRole)
			{
				meetingResetRole.ResetOnMeetingStart();
			}
		}

		if (multiAssignRole.AnotherRole is IRoleSpecialReset specialResetRole)
		{
			specialResetRole.AllReset(targetPlayer);
		}
	}

	private static void replaceVanilaRole(
		SingleRoleBase targetRole,
		PlayerControl targetPlayer)
	{
		if (targetRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is VanillaRoleWrapper)
		{
			RoleManager.Instance.SetRole(
				targetPlayer, RoleTypes.Crewmate);
			return;
		}

		switch (targetPlayer.Data.Role.Role)
		{
			case RoleTypes.Crewmate:
			case RoleTypes.Impostor:
				RoleManager.Instance.SetRole(
					targetPlayer, RoleTypes.Crewmate);
				break;
		}
	}

	private static void resetRole(
		SingleRoleBase targetRole,
		byte targetPlayerId,
		PlayerControl targetPlayer)
	{
		if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
		{
			if (targetRole is IRoleAbility abilityRole)
			{
				abilityRole.Button.OnMeetingStart();
			}
			if (targetRole is IRoleResetMeeting meetingResetRole)
			{
				meetingResetRole.ResetOnMeetingStart();
			}
		}

		if (targetRole is IRoleSpecialReset specialResetRole)
		{
			specialResetRole.AllReset(targetPlayer);
		}
	}

	private static void resetAbility(
		SingleRoleBase targetRole,
		byte targetPlayerId)
	{
		if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
		{
			if (targetRole is IRoleAbility abilityRole)
			{
				abilityRole.Button.OnMeetingStart();
				abilityRole.Button.OnMeetingEnd();
			}
			if (targetRole is IRoleResetMeeting meetingResetRole)
			{
				meetingResetRole.ResetOnMeetingStart();
				meetingResetRole.ResetOnMeetingEnd();
			}
		}
	}
}
