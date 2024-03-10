using System;
using Hazel;
using System.Collections.Generic;

using UnityEngine;

using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;
using ExtremeRoles.Extension.Il2Cpp;

namespace ExtremeRoles.Module.SystemType.Roles;

#nullable enable

public sealed class YokoYashiroSystem : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.YokoYashiro;

	public bool IsDirty { get; private set; } = false;

	public sealed class YashiroInfo(RolePlayerId id)
	{
		public enum StatusType : byte
		{
			Deactive,
			Active,
			Seal
		}

		public RolePlayerId Id { get; init; } = id;
		public StatusType Status { get; set; }
		public float Timer { get; set; } = 0.0f;
		public bool IsDirty { get; set; }

		public void Serialize(in MessageWriter writer)
		{
			writer.Write((byte)this.Status);
			writer.Write(this.Timer);
			this.IsDirty = false;
		}
		public void Deserialize(in MessageReader reader)
		{
			this.Status = (StatusType)reader.ReadByte();
			this.Timer = reader.ReadSingle();
			this.IsDirty = false;
		}
	}

	public sealed class YashiroConsoleBehavior(in YashiroInfo info) : ExtremeConsole.IBehavior
	{
		private static Minigame prefab
		{
			get
			{
				GameObject obj =
					Loader.GetUnityObjectFromPath<GameObject>(
						"THIS IS PLACEHOLDER",
						"assets/roles/yokominigame.prefab");
				return obj.GetComponent<YokoYashiroStatusUpdateMinigame>();
			}
		}

		public float CoolTime => 0.0f;

		public bool IsCheckWall => true;

		public YashiroInfo Info { get; init; } = info;

		public bool CanUse(GameData.PlayerInfo pc)
			=> pc.Object.CanMove && !pc.IsDead;

		public void Use()
		{
			// Idセット処理
			var minigame = MinigameSystem.Create(prefab);

			// ホストにこの社の情報をシンクロするように要請
			ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
				Type, x =>
				{
					x.Write((byte)Ops.Resync);
					this.Info.Id.Serialize(x);
				});

			if (!minigame.IsTryCast<YokoYashiroStatusUpdateMinigame>(out var teroMiniGame))
			{
				throw new ArgumentException("Minigame Missing");
			}

			teroMiniGame!.Info = this.Info;
			teroMiniGame!.Begin(null);
		}
	}

	public enum Ops : byte
	{
		Set,
		Resync,
		Update,
	}

	private readonly Dictionary<RolePlayerId, YashiroInfo> allInfo = new Dictionary<RolePlayerId, YashiroInfo>();
	private readonly Dictionary<RolePlayerId, Vector2> yashiroPos = new Dictionary<RolePlayerId, Vector2>();
	private readonly float activeTime = 0.0f;
	private readonly float sealTime = 0.0f;
	private readonly float range;
	private readonly ExtremeConsoleSystem consoleSystem;
	private readonly RolePlayerIdGenerator gen = new RolePlayerIdGenerator();

	public YokoYashiroSystem(float activeTime, float sealTime, float range)
	{
		this.activeTime = activeTime;
		this.sealTime = sealTime;
		this.range = range * range;

		this.consoleSystem = ExtremeConsoleSystem.Create();

		this.allInfo.Clear();
	}

	public bool IsNearActiveYashiro(Vector2 pos)
		=> this.yashiroPos
			.Where(x =>
				this.allInfo.TryGetValue(x.Key, out var info) &&
				info != null &&
				info.Status == YashiroInfo.StatusType.Active &&
				(x.Value - pos).magnitude <= this.range)
			.Any();

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int count = reader.ReadPackedInt32();
		for (int i = 0; i < count; ++i)
		{
			var id = RolePlayerId.DeserializeConstruct(reader);
			if (!this.allInfo.TryGetValue(id, out var info))
			{
				continue;
			}
			info.Deserialize(reader);
		}
		this.IsDirty = initialState;
	}

	public void Deteriorate(float deltaTime)
	{
		if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
			!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

		if ((MeetingHud.Instance != null ||
			ExileController.Instance != null))
		{

		}

		foreach (var info in this.allInfo.Values)
		{
			if (info.Status is YashiroInfo.StatusType.Deactive ||
				info.Timer == float.MaxValue)
			{
				continue;
			}

			info.Timer -= deltaTime;

			if (info.Timer > 0.0f)
			{
				continue;
			}

			var targetStatus = info.Status switch
			{
				YashiroInfo.StatusType.Active => YashiroInfo.StatusType.Seal,
				_ => YashiroInfo.StatusType.Deactive
			};
			updateYashiroInfo(info, targetStatus);
			info.IsDirty = true;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.allInfo.Count);
		foreach (var info in this.allInfo.Values)
		{
			if (!info.IsDirty)
			{
				continue;
			}

			info.Id.Serialize(writer);
			info.Serialize(writer);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			// ここは全員が通すようにする
			case Ops.Set:
				// 社設置処理
				int controlId = msgReader.ReadPackedInt32();
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();
				this.createYashiro(controlId, new Vector2(x, y));
				this.IsDirty = false;
				break;
			case Ops.Resync:
				var resyncId = RolePlayerId.DeserializeConstruct(msgReader);
				lock (this.allInfo)
				{
					if (this.allInfo.TryGetValue(resyncId, out var resyncInfo))
					{
						resyncInfo.IsDirty = true;
					}
				}
				this.IsDirty = true;
				break;
			case Ops.Update:
				var updateId = RolePlayerId.DeserializeConstruct(msgReader);
				var newStatus = (YashiroInfo.StatusType)msgReader.ReadByte();
				lock (this.allInfo)
				{
					if (this.allInfo.TryGetValue(updateId, out var updateInfo))
					{
						updateYashiroInfo(updateInfo, newStatus);
						updateInfo.IsDirty = true;
					}
				}
				this.IsDirty = true;
				break;
			default:
				return;
		}
	}

	private void createYashiro(in int controlId, in Vector2 pos)
	{
		var id = this.gen.Generate(controlId);
		var info = new YashiroInfo(id);
		var consoleBehavior = new YashiroConsoleBehavior(info);

		var newConsole = this.consoleSystem.CreateConsoleObj(
			pos, "Yashiro", consoleBehavior);

		newConsole.Image!.sprite = Loader.CreateSpriteFromResources(
			Path.TeroristTeroSabotageBomb);

		var colider = newConsole.gameObject.AddComponent<CircleCollider2D>();
		colider.isTrigger = true;
		colider.radius = 0.1f;

		this.allInfo.Add(id, info);
		this.yashiroPos.Add(id, pos);
	}

	public void RpcUpdateNextStatus(YashiroInfo info)
	{
		UpdateNextStatus(info);

		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			Type, x =>
			{
				x.Write((byte)Ops.Update);
				info.Id.Serialize(x);
				x.Write((byte)info.Status);
			});
	}

	public void UpdateNextStatus(in YashiroInfo info)
	{
		var targetStatus = info.Status switch
		{
			YashiroInfo.StatusType.Active => YashiroInfo.StatusType.Seal,
			_ => YashiroInfo.StatusType.Deactive
		};
		updateYashiroInfo(info, targetStatus);
	}

	private void updateYashiroInfo(in YashiroInfo info, YashiroInfo.StatusType targetStatus)
	{
		info.Status = targetStatus;
		switch (targetStatus)
		{
			case YashiroInfo.StatusType.Active:
				info.Timer = this.activeTime;
				break;
			case YashiroInfo.StatusType.Seal:
				info.Timer = this.sealTime;
				break;
			default:
				break;
		}
	}
}