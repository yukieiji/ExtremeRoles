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
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.SystemType.Roles;

#nullable enable

public sealed class YokoYashiroSystem(float activeTime, float sealTime, float range, bool isChangeMeeting) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.YokoYashiro;

	public bool IsDirty { get; private set; } = false;

	private readonly Dictionary<RolePlayerId, YashiroInfo> allInfo = new Dictionary<RolePlayerId, YashiroInfo>();
	private readonly Dictionary<RolePlayerId, Vector2> yashiroPos = new Dictionary<RolePlayerId, Vector2>();
	private readonly float activeTime = activeTime;
	private readonly float sealTime = sealTime;
	private readonly float range = range;
	private readonly bool isChangeMeeting = isChangeMeeting;
	private readonly ExtremeConsoleSystem consoleSystem = ExtremeConsoleSystem.Create();
	private readonly RolePlayerIdGenerator gen = new RolePlayerIdGenerator();

	public sealed class YashiroInfo(RolePlayerId id)
	{
		public enum StatusType : byte
		{
			YashiroDeactive,
			YashiroActive,
			YashiroSeal
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

	public sealed class YashiroConsoleBehavior(in RolePlayerId id) : ExtremeConsole.IBehavior
	{
		private static Minigame prefab
		{
			get
			{
				GameObject obj =
					Loader.GetUnityObjectFromResources<GameObject, ExtremeRoleId>(
						ExtremeRoleId.Yoko,
						Path.GetRoleMinigamePath(ExtremeRoleId.Yoko));
				return obj.GetComponent<YokoYashiroStatusUpdateMinigame>();
			}
		}

		public float CoolTime => 0.0f;

		public bool IsCheckWall => true;

		public RolePlayerId Id { get; init; } = id;

		public bool CanUse(GameData.PlayerInfo pc)
			=> pc.Object.CanMove && !pc.IsDead;

		public void Use()
		{
			// ホストにこの社の情報をシンクロするように要請
			ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
				Type, x =>
				{
					x.Write((byte)Ops.Resync);
					this.Id.Serialize(x);
				});

			var minigame = MinigameSystem.Create(prefab);

			if (!minigame.IsTryCast<YokoYashiroStatusUpdateMinigame>(out var teroMiniGame) ||
				!ExtremeSystemTypeManager.Instance.TryGet<YokoYashiroSystem>(Type, out var system))
			{
				throw new ArgumentException("Minigame Missing");
			}

			teroMiniGame.Info = system.allInfo[this.Id];
			teroMiniGame.Begin(null);
		}
	}

	public enum Ops : byte
	{
		Set,
		Resync,
		Update,
	}

	public static YashiroInfo.StatusType GetNextStatus(YashiroInfo.StatusType curStatus)
		=> curStatus switch
		{
			YashiroInfo.StatusType.YashiroDeactive => YashiroInfo.StatusType.YashiroActive,
			YashiroInfo.StatusType.YashiroActive => YashiroInfo.StatusType.YashiroSeal,
			_ => YashiroInfo.StatusType.YashiroDeactive
		};

	public bool CanSet(Vector2 pos)
		=> !this.yashiroPos.Values
			.Where(x => (x - pos).magnitude <= this.range * 2)
			.Any();

	public bool IsNearActiveYashiro(Vector2 pos)
		=> this.yashiroPos
			.Where(x =>
				this.allInfo.TryGetValue(x.Key, out var info) &&
				info != null &&
				info.Status == YashiroInfo.StatusType.YashiroActive &&
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
		if (!AmongUsClient.Instance.AmHost ||
			AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
			!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return;
		}

		if (!this.isChangeMeeting &&
			(MeetingHud.Instance != null || ExileController.Instance != null))
		{
			return;
		}

		foreach (var info in this.allInfo.Values)
		{
			if (info.Status is YashiroInfo.StatusType.YashiroDeactive ||
				info.Timer == float.MaxValue)
			{
				continue;
			}

			info.Timer -= deltaTime;

			if (info.Timer > 0.0f)
			{
				continue;
			}

			var targetStatus = GetNextStatus(info.Status);
			updateYashiroInfo(info, targetStatus);
			info.IsDirty = true;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

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

	public void RpcSetYashiro(int controlId, Vector2 pos)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type, x =>
			{
				x.Write((byte)Ops.Set);
				x.WritePacked(controlId);
				x.Write(pos.x);
				x.Write(pos.y);
			});
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		int size = 0;
		var dirtyInfo = new List<YashiroInfo>(this.allInfo.Count);
		foreach (var info in this.allInfo.Values)
		{
			if (!info.IsDirty)
			{
				continue;
			}
			size++;
			dirtyInfo.Add(info);
		}

		writer.WritePacked(dirtyInfo.Count);
		foreach (var info in dirtyInfo)
		{
			info.Id.Serialize(writer);
			info.Serialize(writer);
		}
	}

	public void UpdateNextStatus(in YashiroInfo info)
	{
		var targetStatus = GetNextStatus(info.Status);
		updateYashiroInfo(info, targetStatus);
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
		var consoleBehavior = new YashiroConsoleBehavior(id);

		var newConsole = this.consoleSystem.CreateConsoleObj(
			pos, "Yashiro", consoleBehavior);

		newConsole.Image!.sprite = Loader.GetSpriteFromResources(
			ExtremeRoleId.Yoko);

		var colider = newConsole.gameObject.AddComponent<CircleCollider2D>();
		colider.isTrigger = true;
		colider.radius = 0.1f;

		this.allInfo.Add(id, info);
		this.yashiroPos.Add(id, pos);
	}

	private void updateYashiroInfo(in YashiroInfo info, YashiroInfo.StatusType targetStatus)
	{
		info.Status = targetStatus;
		switch (targetStatus)
		{
			case YashiroInfo.StatusType.YashiroActive:
				info.Timer = this.activeTime;
				break;
			case YashiroInfo.StatusType.YashiroSeal:
				info.Timer = this.sealTime;
				break;
			default:
				break;
		}
	}
}