using Hazel;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class YokoYashiroSystem : IDirtableSystemType
{
	public bool IsDirty { get; private set; } = false;

	public sealed class YashiroInfo(int id)
	{
		public enum StatusType : byte
		{
			Deactive,
			Active,
			Seal
		}

		public int Id { get; init; } = id;
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
					Loader.GetUnityObjectFromResources<GameObject>(
						Path.TeroristTeroMinigameAsset,
						Path.TeroristTeroMinigamePrefab);
				return obj.GetComponent<TeroristTeroSabotageMinigame>();
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
		}
	}

	public enum Ops : byte
	{
		Set,
		Resync,
		Update,
	}

	private readonly Dictionary<int, YashiroInfo> allInfo = new Dictionary<int, YashiroInfo>();
	private int id = 0;

	private readonly float activeTime = 0.0f;
	private readonly float sealTime = 0.0f;
	private readonly ExtremeConsoleSystem consoleSystem;

	public YokoYashiroSystem(float activeTime, float sealTime)
	{
		this.activeTime = activeTime;
		this.sealTime = sealTime;

		this.consoleSystem = ExtremeConsoleSystem.Create();

		this.allInfo.Clear();
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int count = reader.ReadPackedInt32();
		for (int i = 0; i < count; ++i)
		{
			int id = reader.ReadPackedInt32();
			if (this.allInfo.TryGetValue(id, out var info))
			{
				info.Deserialize(reader);
			}
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

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
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
			writer.WritePacked(info.Id);

			var prevStatus = info.Status;
			info.Serialize(writer);

			if (prevStatus != info.Status)
			{
				// 設置オブジェクトの色の変更処理
			}
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		int id = msgReader.ReadPackedInt32();
		switch (ops)
		{
			// ここは全員が通すようにする
			case Ops.Set:
				// 社設置処理
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();
				this.setYashiro(id, new Vector2(x, y));
				this.IsDirty = false;
				break;
			case Ops.Resync:
				lock (this.allInfo)
				{
					if (this.allInfo.TryGetValue(id, out var resyncInfo))
					{
						resyncInfo.IsDirty = true;
					}
				}
				this.IsDirty = true;
				break;
			case Ops.Update:
				var newStatus = (YashiroInfo.StatusType)msgReader.ReadByte();
				lock (this.allInfo)
				{
					if (this.allInfo.TryGetValue(id, out var updateInfo))
					{
						updateYashiroInfo(updateInfo, newStatus);
						updateInfo.IsDirty = true;
					}
				}
				this.IsDirty = true;
				break;
			default:
				break;
		}
	}

	private void setYashiro(in int id, in Vector2 pos)
	{
		var info = new YashiroInfo(id);
		var consoleBehavior = new YashiroConsoleBehavior(info);

		var newConsole = this.consoleSystem.CreateConsoleObj(
			pos, "Yashiro", consoleBehavior);

		newConsole.Image!.sprite = Loader.CreateSpriteFromResources(
			Path.TeroristTeroSabotageBomb);

		var colider = newConsole.gameObject.AddComponent<CircleCollider2D>();
		colider.isTrigger = true;
		colider.radius = 0.1f;
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
