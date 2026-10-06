using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Mastermind :
	SingleRoleBase,
	IRoleSpecialSetUp,
	IRoleResetMeeting,
	IRoleMeetingButtonAbility,
	IRoleVoteModifier,
	IRoleWinPlayerModifier,
	IRoleUpdate
{
	public enum MastermindOption
	{
		VoteGainPerTask,
		InfiniteTasks,
		TaskAddProgressThreshold,
		CanSeeImpostor,
		CanSeeNeutral,
		CanSeeLiberal,
	}

	[Flags]
	public enum RoleVisionFlags : byte
	{
		None = 0,
		Impostor = 1 << 0,
		Neutral = 1 << 1,
		Liberal = 1 << 2,
	}

	public int Order => (int)IRoleVoteModifier.ModOrder.MastermindSpecialVote;

	public Sprite AbilityImage => UnityObjectLoader.LoadSpriteFromResources(
		ObjectPath.CaptainSpecialVote);

	private float voteGainPerTask;
	private float? taskAddProgressThreshold;
	private RoleVisionFlags visionFlags;

	private int shortTask;
	private int normalTask;
	private int allTaskNum;

	private HashSet<uint> oldTaskComplete = [];
	private float waitTimer = 0.0f;
	private NetworkedPlayerInfo? cachePlayer;

	private float curChargedVote;
	private byte voteTarget = byte.MaxValue;

	private GridArrange? grid;
	private Dictionary<byte, PoolablePlayer> playerIcons = [];

	private TMPro.TextMeshPro? meetingVoteText;
	private Dictionary<byte, SpriteRenderer> voteCheckMark = [];

	public Mastermind() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.Mastermind,
			ColorPalette.MastermindSlateBlue,
			RoleProp.CanCallMeeting | RoleProp.CanUseAdmin))
	{ }

	public static void UseAbility(ref Hazel.MessageReader reader)
	{
		byte rolePlayerId = reader.ReadByte();
		byte targetPlayerId = reader.ReadByte();

		Mastermind? mastermind = ExtremeRoleManager.GetSafeCastedRole<Mastermind>(rolePlayerId);
		mastermind?.SetTargetVote(targetPlayerId);
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

		int addVoteNum = (int)Math.Floor(this.curChargedVote);
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

		int addVoteNum = (int)Math.Floor(this.curChargedVote);
		if (addVoteNum > 0)
		{
			yield return new VoteInfo(rolePlayer.PlayerId, this.voteTarget, addVoteNum);
		}
	}

	public void ResetModifier()
	{
		if (this.voteTarget != byte.MaxValue)
		{
			this.curChargedVote = 0;
		}
		this.voteTarget = byte.MaxValue;
		this.voteCheckMark.Clear();
	}

	public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "captainSpecialVote");

	public Action CreateAbilityAction(PlayerVoteArea instance)
	{
		void setTarget()
		{
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.MastermindAbility))
			{
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
				checkMark.name = $"mastermind_SpecialVoteCheckMark_{instance.PlayerId}";
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

	public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance) => Math.Floor(this.curChargedVote) < 1.0f;

	public void IntroBeginSetUp() { }

	public void IntroEndSetUp()
	{
		var bottomLeft = new GameObject("BottomLeft");
		bottomLeft.transform.SetParent(
			HudManager.Instance.UseButton.transform.parent.parent);
		var aspectPosition = bottomLeft.AddComponent<AspectPosition>();
		aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
		aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
		aspectPosition.DistanceFromEdge = new Vector3(0.375f, 0.35f);
		aspectPosition.AdjustPosition();

		this.grid = bottomLeft.AddComponent<GridArrange>();
		this.grid.CellSize = new Vector2(0.625f, 0.75f);
		this.grid.MaxColumns = 10;
		this.grid.Alignment = GridArrange.StartAlign.Right;
		this.grid.cells = new();

		this.playerIcons = Player.CreatePlayerIcon(
			bottomLeft.transform, Vector3.one * 0.275f);
		updateShowIcon();
	}

	public void ResetOnMeetingStart()
	{
		foreach (var poolPlayer in this.playerIcons.Values)
		{
			if (poolPlayer != null)
			{
				poolPlayer.gameObject.SetActive(false);
			}
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		updateShowIcon();
	}

	public override Color GetTargetRoleSeeColor(
		SingleRoleBase targetRole,
		byte targetPlayerId)
	{
		if (targetRole.IsImpostor() && this.visionFlags.HasFlag(RoleVisionFlags.Impostor))
		{
			return Palette.ImpostorRed;
		}
		else if (targetRole.IsNeutral() && this.visionFlags.HasFlag(RoleVisionFlags.Neutral))
		{
			return ColorPalette.NeutralColor;
		}
		else if (targetRole.IsLiberal() && this.visionFlags.HasFlag(RoleVisionFlags.Liberal))
		{
			return ColorPalette.LiberalColor;
		}

		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (MeetingHud.Instance != null)
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

			meetingVoteText.text = Tr.GetString(
				"captainVoteStatus",
				Math.Floor(this.curChargedVote) < 1.0f ? Tr.GetString("cannotDo") : Tr.GetString("canDo"),
				this.curChargedVote);
			meetingVoteText.gameObject.SetActive(true);
		}

		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		if (cachePlayer == null)
		{
			cachePlayer = GameData.Instance.GetPlayerById(rolePlayer.PlayerId);
		}
		if (cachePlayer.IsInValid() || cachePlayer.Tasks.Count == 0)
		{
			return;
		}

		if (this.waitTimer >= 0.0f)
		{
			this.waitTimer -= Time.deltaTime;
			return;
		}

		this.waitTimer = 1.0f;

		List<uint> curTaskComplete = [];
		for (int i = 0; i < cachePlayer.Tasks.Count; ++i)
		{
			var task = cachePlayer.Tasks[i];
			if (task.Complete)
			{
				curTaskComplete.Add(task.Id);
			}
		}

		if (curTaskComplete.Count == 0 || curTaskComplete.Count == this.oldTaskComplete.Count)
		{
			return;
		}

		int newCompletedNum = curTaskComplete.Count - this.oldTaskComplete.Count;
		if (newCompletedNum > 0)
		{
			this.curChargedVote += this.voteGainPerTask * newCompletedNum;
		}

		this.oldTaskComplete = [..curTaskComplete];

		if (!this.taskAddProgressThreshold.HasValue)
		{
			return;
		}
		
		float gage = Player.GetPlayerTaskGage(cachePlayer);
		if (gage < this.taskAddProgressThreshold.Value)
		{
			return;
		}

		byte playerId = cachePlayer.PlayerId;
		for (int i = 0; i < cachePlayer.Tasks.Count; ++i)
		{
			int taskTarget = RandomGenerator.Instance.Next(0, this.allTaskNum);
			int taskIndex;
			if (taskTarget < this.shortTask)
			{
				taskIndex = GameSystem.GetRandomShortTaskId();
			}
			else if (taskTarget < this.normalTask)
			{
				taskIndex = GameSystem.GetRandomCommonTaskId();
			}
			else
			{
				taskIndex = GameSystem.GetRandomLongTask();
			}
			GameSystem.RpcReplaceNewTask(playerId, i, taskIndex);
		}
		this.oldTaskComplete.Clear();
	}

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerContainer winner)
	{
		if (rolePlayerInfo.IsDead || rolePlayerInfo.Disconnected)
		{
			return;
		}

		var allPlayers = GameData.Instance.AllPlayers;
		int aliveCount = 0;
		foreach (var player in allPlayers.GetFastEnumerator())
		{
			if (player.IsAlive())
			{
				aliveCount++;
			}
		}

		if (aliveCount > 3)
		{
			return;
		}

		winner.AllClear();
		foreach (var player in allPlayers.GetFastEnumerator())
		{
			if (player.IsAlive() &&
				ExtremeRoleManager.TryGetSafeCastedRole<Mastermind>(player.PlayerId, out var _))
			{
				winner.Add(player);
			}
		}
		ExtremeRolesPlugin.ShipState.SetGameOverReason(
			(GameOverReason)RoleGameOverReason.MastermindAlive);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			MastermindOption.VoteGainPerTask,
			1.0f, 0.1f, 10.0f, 0.1f,
			format: OptionUnit.VoteNum);

		var infiniteTaskOpt = factory.CreateBoolOption(
			MastermindOption.InfiniteTasks,
			true);

		factory.CreateIntOption(
			MastermindOption.TaskAddProgressThreshold,
			50, 0, 100, 10,
			new InvertActive(infiniteTaskOpt),
			format: OptionUnit.Percentage);

		factory.CreateBoolOption(
			MastermindOption.CanSeeImpostor,
			true);

		factory.CreateBoolOption(
			MastermindOption.CanSeeNeutral,
			true);

		factory.CreateBoolOption(
			MastermindOption.CanSeeLiberal,
			true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		this.voteGainPerTask = loader.GetValue<MastermindOption, float>(MastermindOption.VoteGainPerTask);

		this.taskAddProgressThreshold =
			loader.GetValue<MastermindOption, bool>(MastermindOption.InfiniteTasks) ?
			loader.GetValue<MastermindOption, int>(MastermindOption.TaskAddProgressThreshold) / 100.0f : null;

		this.visionFlags = RoleVisionFlags.None;
		if (loader.GetValue<MastermindOption, bool>(MastermindOption.CanSeeImpostor))
		{
			this.visionFlags |= RoleVisionFlags.Impostor;
		}
		if (loader.GetValue<MastermindOption, bool>(MastermindOption.CanSeeNeutral))
		{
			this.visionFlags |= RoleVisionFlags.Neutral;
		}
		if (loader.GetValue<MastermindOption, bool>(MastermindOption.CanSeeLiberal))
		{
			this.visionFlags |= RoleVisionFlags.Liberal;
		}

		var option = GameOptionsManager.Instance.CurrentGameOptions;
		this.shortTask = option.GetInt(Int32OptionNames.NumShortTasks);
		this.normalTask = this.shortTask + option.GetInt(Int32OptionNames.NumCommonTasks);
		this.allTaskNum = this.normalTask + option.GetInt(Int32OptionNames.NumLongTasks);

		this.curChargedVote = 0.0f;
		this.voteTarget = byte.MaxValue;
		this.voteCheckMark = new Dictionary<byte, SpriteRenderer>();
		this.playerIcons = new Dictionary<byte, PoolablePlayer>();
		this.oldTaskComplete = new HashSet<uint>();

		_ = MastermindNoticeSystem.CreateOrGet();
	}

	private void updateShowIcon()
	{
		foreach (var (playerId, poolPlayer) in this.playerIcons)
		{
			if (playerId == PlayerControl.LocalPlayer.PlayerId ||
				!ExtremeRoleManager.TryGetRole(playerId, out var role))
			{
				continue;
			}

			bool show = (role.IsImpostor() && this.visionFlags.HasFlag(RoleVisionFlags.Impostor)) ||
						(role.IsNeutral() && this.visionFlags.HasFlag(RoleVisionFlags.Neutral)) ||
						(role.IsLiberal() && this.visionFlags.HasFlag(RoleVisionFlags.Liberal));

			if (show)
			{
				poolPlayer.transform.localScale = Vector3.one * 0.275f;
				poolPlayer.gameObject.SetActive(true);
			}
			else
			{
				poolPlayer.gameObject.SetActive(false);
			}
		}
		if (this.grid == null)
		{
			return;
		}
		this.grid.ArrangeChilds();
	}
}
