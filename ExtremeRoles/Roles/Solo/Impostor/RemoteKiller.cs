using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class RemoteKillerStatus : IStatusModel, IStatusMovable
{
	public bool CanMove { get; set; } = true;
}

public sealed class RemoteKiller :
	SingleRoleBase,
	IRoleAbility,
	IRoleUpdate,
	IRoleResetMeeting
{
	public enum RemoteKillerOption
	{
		RobRange,
		RobActiveTime,
		ContactPlayerCount,
		PurgeTime,
	}

	public enum RemoteKillerRpc : byte
	{
		RobSuccess,
		PurgeStart,
		PurgeCancel,
		PurgeExecute,
	}

	public sealed class RemoteKillerReportSerializer : IStringSerializer
	{
		public StringSerializerType Type => StringSerializerType.RemoteKillerNotebookStolen;
		public bool IsRpc { get; set; } = false;

		private readonly List<byte> contactPlayerIds = new List<byte>();

		public RemoteKillerReportSerializer() { }

		public RemoteKillerReportSerializer(List<byte> playerIds)
		{
			this.contactPlayerIds = playerIds;
		}

		public void Serialize(RPCOperator.RpcCaller caller)
		{
			caller.WriteByte((byte)this.contactPlayerIds.Count);
			foreach (byte id in this.contactPlayerIds)
			{
				caller.WriteByte(id);
			}
		}

		public void Deserialize(MessageReader reader)
		{
			byte count = reader.ReadByte();
			this.contactPlayerIds.Clear();
			for (int i = 0; i < count; i++)
			{
				this.contactPlayerIds.Add(reader.ReadByte());
			}
		}

		public override string ToString()
		{
			string header = Tr.GetString("remoteKillerNotebookStolenReport");
			if (this.contactPlayerIds.Count == 0)
			{
				return header;
			}

			List<string> names = new List<string>();
			foreach (byte pId in this.contactPlayerIds)
			{
				var p = Player.GetPlayerControlById(pId);
				if (p != null && p.Data != null)
				{
					names.Add(p.Data.PlayerName);
				}
			}

			if (names.Count == 0)
			{
				return header;
			}

			return $"{header}\n{string.Join("\n", names)}";
		}
	}

	public ExtremeAbilityButton? Button
	{
		get => this.internalButton;
		set
		{
			if (value is not ExtremeMultiModalAbilityButton button)
			{
				throw new ArgumentException("This role uses multimodal ability");
			}
			this.internalButton = button;
		}
	}
	private ExtremeMultiModalAbilityButton? internalButton;

	public override IStatusModel? Status => this.status;

	public bool IsPurging { get; set; } = false;

	private RemoteKillerStatus? status;

	private ReusableActivatingBehavior? robBehavior;
	private ActivatingCountBehavior? purgeBehavior;
	private ShapeShiftMinigameWrapper? minigame;

	private PlayerControl? currentRobTarget;
	private PlayerControl? selectedPurgeTarget;

	private float robRange;
	private float robActiveTime;
	private int contactPlayerCount;
	private float purgeTime;

	private readonly HashSet<byte> executionTargets = new HashSet<byte>();
	private readonly HashSet<byte> pendingReports = new HashSet<byte>();
	private readonly Dictionary<byte, HashSet<byte>> taskPhaseContacts = new Dictionary<byte, HashSet<byte>>();

	public RemoteKiller() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.RemoteKiller))
	{ }

	public static void RpcHandle(ref MessageReader reader)
	{
		RemoteKillerRpc ops = (RemoteKillerRpc)reader.ReadByte();
		byte rolePlayerId = reader.ReadByte();

		var remoteKiller = ExtremeRoleManager.GetSafeCastedRole<RemoteKiller>(rolePlayerId);
		if (remoteKiller is null)
		{
			return;
		}

		switch (ops)
		{
			case RemoteKillerRpc.RobSuccess:
				byte targetId = reader.ReadByte();
				remoteKiller.executionTargets.Add(targetId);
				remoteKiller.pendingReports.Add(targetId);
				if (!remoteKiller.taskPhaseContacts.ContainsKey(targetId))
				{
					remoteKiller.taskPhaseContacts[targetId] = new HashSet<byte>();
				}
				break;

			case RemoteKillerRpc.PurgeStart:
				remoteKiller.IsPurging = true;
				if (remoteKiller.status is not null)
				{
					remoteKiller.status.CanMove = false;
				}
				break;

			case RemoteKillerRpc.PurgeCancel:
				remoteKiller.IsPurging = false;
				if (remoteKiller.status is not null)
				{
					remoteKiller.status.CanMove = true;
				}
				break;

			case RemoteKillerRpc.PurgeExecute:
				byte executedTarget = reader.ReadByte();
				remoteKiller.IsPurging = false;
				if (remoteKiller.status is not null)
				{
					remoteKiller.status.CanMove = true;
				}

				if (remoteKiller.executionTargets.Contains(executedTarget))
				{
					remoteKiller.executionTargets.Remove(executedTarget);
					Player.RpcUncheckMurderPlayer(executedTarget, executedTarget, 0);
				}
				break;
		}

		EventManager.Instance.Invoke(ModEvent.VisualUpdate);
	}

	public void CreateAbility()
	{
		var loader = this.Loader;

		float coolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);
		int purgeCount = loader.GetValue<RoleAbilityCommonOption, int>(
			RoleAbilityCommonOption.AbilityCount);

		this.robActiveTime = loader.GetValue<RemoteKillerOption, float>(
			RemoteKillerOption.RobActiveTime);
		this.purgeTime = loader.GetValue<RemoteKillerOption, float>(
			RemoteKillerOption.PurgeTime);

		this.robBehavior = new ReusableActivatingBehavior(
			text: Tr.GetString("remoteKillerRob"),
			img: UnityObjectLoader.LoadSpriteFromResources(ObjectPath.UmbrerFeatVirus),
			canUse: isUseRob,
			ability: robStart,
			canActivating: isRobCheck,
			abilityOff: robCleanUp,
			forceAbilityOff: robForceCleanUp);
		this.robBehavior.SetCoolTime(coolTime);
		this.robBehavior.ActiveTime = this.robActiveTime;

		this.purgeBehavior = new ActivatingCountBehavior(
			text: Tr.GetString("remoteKillerPurge"),
			img: UnityObjectLoader.LoadSpriteFromResources(ObjectPath.SucideSprite),
			canUse: isUsePurge,
			ability: purgeOpenMenu,
			canActivating: isPurgeCheck,
			abilityOff: purgeCleanUp,
			forceAbilityOff: purgeForceCleanUp,
			isReduceOnActive: false);
		this.purgeBehavior.SetCoolTime(coolTime);
		this.purgeBehavior.ActiveTime = this.purgeTime;
		this.purgeBehavior.SetAbilityCount(purgeCount);

		this.Button = new ExtremeMultiModalAbilityButton(
			new RoleButtonActivator(),
			KeyCode.F,
			this.robBehavior,
			this.purgeBehavior);
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void ResetOnMeetingStart()
	{		if (this.IsPurging)
		{
			purgeForceCleanUp();
		}

		this.minigame?.Reset();

		bool shouldSendReport = PlayerControl.LocalPlayer.PlayerId == this.GameControlId || AmongUsClient.Instance.AmHost;

		foreach (byte targetId in this.executionTargets.ToList())
		{
			var target = Player.GetPlayerControlById(targetId);
			if (target != null && target.Data != null && !target.Data.IsDead && !target.Data.Disconnected)
			{
				if (this.pendingReports.Contains(targetId))
				{
					if (shouldSendReport)
					{
						if (this.taskPhaseContacts.TryGetValue(targetId, out var contactSet))
						{
							var pickedIds = contactSet
								.OrderBy(_ => RandomGenerator.Instance.Next())
								.Take(this.contactPlayerCount)
								.ToList();

							MeetingReporter.RpcAddTargetMeetingChatReport(
								targetId, new RemoteKillerReportSerializer(pickedIds));
						}
						else
						{
							MeetingReporter.RpcAddTargetMeetingChatReport(
								targetId, new RemoteKillerReportSerializer(new List<byte>()));
						}
					}

					this.pendingReports.Remove(targetId);
				}
			}

			if (this.taskPhaseContacts.ContainsKey(targetId))
			{
				this.taskPhaseContacts[targetId].Clear();
			}
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		if (this.status is not null)
		{
			this.status.CanMove = true;
		}
		this.IsPurging = false;
		this.selectedPurgeTarget = null;
	}

	public void Update(PlayerControl rolePlayer)
	{
		// Clean up dead or disconnected execution targets
		this.executionTargets.RemoveWhere(id =>
		{
			var p = GameData.Instance.GetPlayerById(id);
			return p == null || p.IsDead || p.Disconnected;
		});

		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		foreach (byte targetId in this.executionTargets)
		{
			var targetInfo = GameData.Instance.GetPlayerById(targetId);
			if (targetInfo == null || targetInfo.IsDead || targetInfo.Disconnected || !targetInfo.Object)
			{
				continue;
			}

			Vector2 targetPos = targetInfo.Object.GetTruePosition();

			if (!this.taskPhaseContacts.TryGetValue(targetId, out var contacts))
			{
				contacts = new HashSet<byte>();
				this.taskPhaseContacts[targetId] = contacts;
			}

			foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (playerInfo.IsInValid() || playerInfo.PlayerId == targetId || !playerInfo.Object || playerInfo.Object.inVent)
				{
					continue;
				}

				Vector2 diff = playerInfo.Object.GetTruePosition() - targetPos;
				float dist = diff.magnitude;

				if (dist <= this.robRange &&
					!PhysicsHelpers.AnyNonTriggersBetween(
						targetPos, diff.normalized, dist, Constants.ShipAndObjectsMask))
				{
					contacts.Add(playerInfo.PlayerId);
				}
			}
		}
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.executionTargets.Contains(targetPlayerId))
		{
			return Design.ColoredString(Palette.ImpostorRed, " ▼");
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 2, 15, 1f);

		factory.CreateFloatOption(
			RemoteKillerOption.RobRange,
			1.0f, 0.1f, 4.0f, 0.1f);

		factory.CreateFloatOption(
			RemoteKillerOption.RobActiveTime,
			2.0f, 0.5f, 10.0f, 0.5f,
			format: OptionUnit.Second);

		factory.CreateIntOption(
			RemoteKillerOption.ContactPlayerCount,
			1, 1, 15, 1);

		factory.CreateFloatOption(
			RemoteKillerOption.PurgeTime,
			5.0f, 0.5f, 30.0f, 0.5f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		this.status = new RemoteKillerStatus();

		var loader = this.Loader;
		this.robRange = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.RobRange);
		this.robActiveTime = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.RobActiveTime);
		this.contactPlayerCount = loader.GetValue<RemoteKillerOption, int>(RemoteKillerOption.ContactPlayerCount);
		this.purgeTime = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.PurgeTime);

		this.executionTargets.Clear();
		this.pendingReports.Clear();
		this.taskPhaseContacts.Clear();
		this.IsPurging = false;
		this.selectedPurgeTarget = null;
		this.currentRobTarget = null;
	}

	private bool isUseRob()
	{
		if (!IRoleAbility.IsCommonUse())
		{
			return false;
		}

		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		var target = Helper.Player.GetClosestPlayerInRange(localPlayer, this, this.robRange);
		if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
		{
			return false;
		}

		if (this.executionTargets.Contains(target.PlayerId))
		{
			return false;
		}

		if (ExtremeRoleManager.TryGetRole(target.PlayerId, out var targetRole) &&
			this.IsSameTeam(targetRole))
		{
			return false;
		}

		return true;
	}

	private bool robStart()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		this.currentRobTarget = Helper.Player.GetClosestPlayerInRange(localPlayer, this, this.robRange);
		return this.currentRobTarget != null;
	}

	private bool isRobCheck()
	{
		if (this.currentRobTarget == null ||
			this.currentRobTarget.Data == null ||
			this.currentRobTarget.Data.IsDead ||
			this.currentRobTarget.Data.Disconnected)
		{
			return false;
		}

		return Helper.Player.IsPlayerInRangeAndDrawOutLine(
			PlayerControl.LocalPlayer,
			this.currentRobTarget,
			this,
			this.robRange);
	}

	private void robCleanUp()
	{
		if (this.currentRobTarget != null)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.currentRobTarget.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRpc.RobSuccess);
				caller.WriteByte(localId);
				caller.WriteByte(targetId);
			}
		}
		this.currentRobTarget = null;
	}

	private void robForceCleanUp()
	{
		this.currentRobTarget = null;
	}

	private bool isUsePurge()
	{
		if (!IRoleAbility.IsCommonUseWithMinigame())
		{
			return false;
		}

		return this.executionTargets.Any(id =>
		{
			var p = Player.GetPlayerControlById(id);
			return p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected;
		});
	}

	private bool purgeOpenMenu()
	{
		if (this.selectedPurgeTarget != null)
		{
			return true;
		}

		this.minigame ??= new ShapeShiftMinigameWrapper();
		return this.minigame.IsOpen || this.minigame.OpenUi(
			this.onPurgeTargetSelected,
			p => p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected && this.executionTargets.Contains(p.PlayerId));
	}

	private void onPurgeTargetSelected(PlayerControl target)
	{
		if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
		{
			return;
		}

		if (!this.executionTargets.Contains(target.PlayerId))
		{
			return;
		}

		this.selectedPurgeTarget = target;
		byte localId = PlayerControl.LocalPlayer.PlayerId;
		byte targetId = target.PlayerId;

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
		{
			caller.WriteByte((byte)RemoteKillerRpc.PurgeStart);
			caller.WriteByte(localId);
			caller.WriteByte(targetId);
		}

		if (this.Button != null && this.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			button.OnClick.Invoke();
		}
	}

	private bool isPurgeCheck()
	{
		if (this.selectedPurgeTarget == null ||
			this.selectedPurgeTarget.Data == null ||
			this.selectedPurgeTarget.Data.IsDead ||
			this.selectedPurgeTarget.Data.Disconnected ||
			PlayerControl.LocalPlayer.Data.IsDead ||
			MeetingHud.Instance != null)
		{
			return false;
		}

		return true;
	}

	private void purgeCleanUp()
	{
		if (this.selectedPurgeTarget != null)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.selectedPurgeTarget.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRpc.PurgeExecute);
				caller.WriteByte(localId);
				caller.WriteByte(targetId);
			}
		}

		this.selectedPurgeTarget = null;
	}

	private void purgeForceCleanUp()
	{
		if (this.IsPurging)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRpc.PurgeCancel);
				caller.WriteByte(localId);
			}
		}

		this.selectedPurgeTarget = null;
	}
}
