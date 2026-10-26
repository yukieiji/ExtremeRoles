
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerRole :
	SingleRoleBase,
	IRoleAbility,
	IRoleUpdate
{
	public enum RemoteKillerOption
	{
		RobRange,
		RobActiveTime,
		ContactPlayerCount,
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

			var builder = new StringBuilder(header);
			builder.AppendLine();

			foreach (byte pId in this.contactPlayerIds)
			{
				var p = Player.GetPlayerControlById(pId);
				if (p != null && p.Data != null)
				{
					builder.AppendLine($" - {p.Data.DefaultOutfit.PlayerName}");
				}
			}

			return builder.ToString();
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

	public bool IsPurging => this.statusModel?.IsPurging ?? false;

	private RemoteKillerStatusModel? statusModel;
	private RemoteKillerRobHandler? robHandler;
	private RemoteKillerPurgeHandler? purgeHandler;

	public RemoteKillerRole() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.RemoteKiller))
	{
	}

	public static void RpcHandle(ref MessageReader reader)
	{
		RemoteKillerRpc ops = (RemoteKillerRpc)reader.ReadByte();
		byte rolePlayerId = reader.ReadByte();
		if (!ExtremeRoleManager.TryGetSafeCastedRole<RemoteKillerRole>(rolePlayerId, out var remoteKiller))
		{
			return;
		}

		remoteKiller.statusModel?.SetPurging(ops is RemoteKillerRpc.PurgeStart);
	}

	public void CreateAbility()
	{
		if (this.robHandler == null || this.purgeHandler == null)
		{
			this.RoleSpecificInit();
		}

		var loader = this.Loader;

		float coolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);
		float robActiveTime = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.RobActiveTime);
		var robBehavior = this.robHandler!.CreateBehavior(coolTime, robActiveTime);


		int purgeCount = loader.GetValue<RoleAbilityCommonOption, int>(
			RoleAbilityCommonOption.AbilityCount);
		float purgeTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime);
		var purgeBehavior = this.purgeHandler!.CreateBehavior(coolTime, purgeTime, purgeCount);

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
		var localPlayer = PlayerControl.LocalPlayer;

		if (MeetingHud.Instance == null || 
			this.statusModel == null || 
			localPlayer == null)
		{
			return;
		}

		if (this.IsPurging)
		{
			this.purgeHandler?.PurgeForceCleanUp();
		}

		this.purgeHandler?.ResetMinigame();

		foreach (byte targetId in this.statusModel.ExecutionTargets.ToList())
		{
			var target = Player.GetPlayerControlById(targetId);
			if (target.IsInValid() ||
				!this.statusModel.AddChatSended(targetId) ||
				!this.statusModel.TaskPhaseContacts.TryGetValue(targetId, out var contactSet))
			{
				continue;
			}

			var pickedIds = contactSet
				.OrderBy(_ => RandomGenerator.Instance.Next())
				.Take(this.statusModel.ContactPlayerCount)
				.ToList();

			MeetingReporter.RpcAddTargetMeetingChatReport(
				targetId, new RemoteKillerReportSerializer(pickedIds));
		}
		this.statusModel.ClearTaskPhaseContacts();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.statusModel?.SetPurging(false);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.statusModel is null || this.robHandler is null)
		{
			return;
		}

		// 死亡または切断された執行対象を削除（蘇生時に再度ターゲット可能にするため）
		var deadTargets = this.statusModel.ExecutionTargets.Where(id =>
		{
			var p = GameData.Instance.GetPlayerById(id);
			return p.IsInValid();
		}).ToList();

		foreach (byte deadId in deadTargets)
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
			if (this.statusModel.IsChatSended(targetId))
			{
				continue;
			}
			this.robHandler.RecordTargetContacts(targetId);
		}
	}

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
		=> this.statusModel != null && this.statusModel.HasExecutionTarget(targetPlayerId) ?
			Design.ColoredString(Palette.ImpostorRed, " ▼") :
			base.GetRolePlayerNameTag(targetRole, targetPlayerId);

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 2, 15, 5f);

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
	}

	protected override void RoleSpecificInit()
	{
		if (this.statusModel != null && this.robHandler != null && this.purgeHandler != null)
		{
			return;
		}

		var loader = this.Loader;
		float robRange = loader.GetValue<RemoteKillerOption, float>(RemoteKillerOption.RobRange);
		int contactPlayerCount = loader.GetValue<RemoteKillerOption, int>(RemoteKillerOption.ContactPlayerCount);

		this.statusModel = new RemoteKillerStatusModel(robRange, contactPlayerCount);
		this.robHandler = new RemoteKillerRobHandler(this.statusModel, this);
		this.purgeHandler = new RemoteKillerPurgeHandler(this.statusModel, this);
	}

	// 勝手に読み込みが入って色々と上書きされておかしくなるので読み込みだけ無効にして、セットアップ処理だけ入れとく
	public void RoleAbilityInit()
	{
		this.Button?.OnMeetingEnd();
	}
}
