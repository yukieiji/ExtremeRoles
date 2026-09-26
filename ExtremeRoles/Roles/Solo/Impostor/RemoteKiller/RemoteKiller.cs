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
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerRole :
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
		PurgeStart,
		PurgeCancel,
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

	public override IStatusModel? Status => this.statusModel;

	public bool IsPurging => this.statusModel.IsPurging;

	private readonly RemoteKillerStatusModel statusModel = new RemoteKillerStatusModel();
	private RemoteKillerRobHandler robHandler;
	private RemoteKillerPurgeHandler purgeHandler;

	public RemoteKillerRole() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.RemoteKiller))
	{
		this.robHandler = new RemoteKillerRobHandler(this.statusModel, this);
		this.purgeHandler = new RemoteKillerPurgeHandler(this.statusModel, this);
	}

	public static void RpcHandle(ref MessageReader reader)
	{
		RemoteKillerRpc ops = (RemoteKillerRpc)reader.ReadByte();
		byte rolePlayerId = reader.ReadByte();

		var remoteKiller = ExtremeRoleManager.GetSafeCastedRole<RemoteKillerRole>(rolePlayerId);
		if (remoteKiller is null)
		{
			return;
		}

		switch (ops)
		{
			case RemoteKillerRpc.PurgeStart:
				remoteKiller.statusModel.SetPurging(true);
				remoteKiller.statusModel.CanMove = false;
				break;

			case RemoteKillerRpc.PurgeCancel:
				remoteKiller.statusModel.SetPurging(false);
				remoteKiller.statusModel.CanMove = true;
				break;
		}
	}

	public void CreateAbility()
	{
		var loader = this.Loader;

		float coolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);
		int purgeCount = loader.GetValue<RoleAbilityCommonOption, int>(
			RoleAbilityCommonOption.AbilityCount);

		var robBehavior = this.robHandler.CreateBehavior(coolTime);
		var purgeBehavior = this.purgeHandler.CreateBehavior(coolTime, purgeCount);

		this.Button = new ExtremeMultiModalAbilityButton(
			new RoleButtonActivator(),
			KeyCode.F,
			robBehavior,
			purgeBehavior);
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public bool IsAbilityUseWithMinigame() => IRoleAbility.IsCommonUseWithMinigame();

	public void ResetOnMeetingStart()
	{
		if (this.IsPurging)
		{
			this.purgeHandler.PurgeForceCleanUp();
		}

		this.purgeHandler.ResetMinigame();

		bool shouldSendReport = PlayerControl.LocalPlayer.PlayerId == this.GameControlId || AmongUsClient.Instance.AmHost;

		foreach (byte targetId in this.statusModel.ExecutionTargets.ToList())
		{
			var target = Player.GetPlayerControlById(targetId);
			if (target != null && target.Data != null && !target.Data.IsDead && !target.Data.Disconnected)
			{
				if (this.statusModel.IsPendingReport(targetId))
				{
					if (shouldSendReport)
					{
						if (this.statusModel.TaskPhaseContacts.TryGetValue(targetId, out var contactSet))
						{
							var pickedIds = contactSet
								.OrderBy(_ => RandomGenerator.Instance.Next())
								.Take(this.statusModel.ContactPlayerCount)
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

					this.statusModel.RemovePendingReport(targetId);
				}
			}

			this.statusModel.ClearTaskPhaseContacts(targetId);
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.statusModel.CanMove = true;
		this.statusModel.SetPurging(false);
	}

	public void Update(PlayerControl rolePlayer)
	{
		// 死亡または切断された執行対象を削除（蘇生時に再度ターゲット可能にするため）
		var deadTargets = this.statusModel.ExecutionTargets.Where(id =>
		{
			var p = GameData.Instance.GetPlayerById(id);
			return p == null || p.IsDead || p.Disconnected;
		}).ToList();

		foreach (var deadId in deadTargets)
		{
			this.statusModel.RemoveExecutionTarget(deadId);
		}

		// タスクフェーズ中以外は接触記録を行わない
		if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

		// タスクフェーズ中、各執行対象の近くに接近した他プレイヤーを記録（会議開始時の報告用）
		foreach (byte targetId in this.statusModel.ExecutionTargets)
		{
			this.robHandler.RecordTargetContacts(targetId);
		}
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.statusModel.HasExecutionTarget(targetPlayerId))
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
		var loader = this.Loader;
		float robRange = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.RobRange);
		float robActiveTime = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.RobActiveTime);
		int contactPlayerCount = loader.GetValue<RemoteKillerOption, int>(RemoteKillerOption.ContactPlayerCount);
		float purgeTime = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.PurgeTime);

		this.statusModel.Reset();
		this.statusModel.SetOptions(robRange, robActiveTime, contactPlayerCount, purgeTime);
	}
}
