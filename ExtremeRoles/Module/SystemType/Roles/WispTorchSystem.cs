using UnityEngine;

using Hazel;
using System;
using System.Linq;
using System.Collections.Generic;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Compat;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class WispTorchSystem : IExtremeSystemType
{
	public sealed class Torch
	{
		private readonly GameObject body;

		public Torch(int id, float range, Vector2 pos)
		{
			this.body = new GameObject("Torch");
			this.body.transform.position = new Vector3(
				pos.x, pos.y, (pos.y / 1000f));
			if (CompatModManager.Instance.TryGetModMap(out var modMap))
			{
				modMap!.AddCustomComponent(
					this.body,
					Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
			}
			TorchBehavior torch = this.body.AddComponent<TorchBehavior>();
			torch.UsableDistance = range;
			torch.GroupId = id;
			this.body.SetActive(true);
		}
		public void Remove()
		{
			UnityObject.Destroy(this.body);
		}
	}

	public sealed class TorchGroup
	{
		public IReadOnlySet<byte> HasPlayer => this.hasPlayer;

		public readonly int GroupId;
		public readonly int WispId;

		public float Timer { get; private set; } = 0.0f;

		private readonly HashSet<byte> hasPlayer = new HashSet<byte>();
		private readonly IReadOnlyList<Torch> placedTorch;

		public TorchGroup(int groupId, int callWispId, int num, float range)
		{
			this.GroupId = groupId;
			this.WispId = callWispId;

			byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
				ByteOptionNames.MapId);
			int playerNum = CachedPlayerControl.AllPlayerControls.Count;

			int clampedNum = Math.Clamp(num, 0, playerNum);
			ShipStatus ship = CachedShipStatus.Instance;

			IEnumerable<CachedPlayerControl> target =
				CachedPlayerControl.AllPlayerControls.OrderBy(
					x => RandomGenerator.Instance.Next()).Take(clampedNum);

			var placeTorch = new List<Torch>();

			foreach (CachedPlayerControl player in target)
			{
				byte playerId = player.PlayerId;

				List<Vector2> placePos = new List<Vector2>();

				if (CompatModManager.Instance.TryGetModMap(out var modMap))
				{
					placePos = modMap!.GetSpawnPos(playerId);
				}
				else
				{
					switch (mapId)
					{
						case 0:
						case 1:
						case 2:
						case 3:
							Vector2 baseVec = Vector2.up;
							baseVec = baseVec.Rotate(
								(float)(playerId - 1) * (360f / (float)playerNum));
							Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(
								0f, 0.3636f);
							placePos.Add(ship.InitialSpawnCenter + offset);
							placePos.Add(ship.MeetingSpawnCenter + offset);
							break;
						case 4:
							placePos = GameSystem.GetAirShipRandomSpawn();
							break;
					}
				}
				var newTorch = new Torch(groupId, range, placePos[
					RandomGenerator.Instance.Next(0, placePos.Count)]);
				placeTorch.Add(newTorch);
			}
			this.placedTorch = placeTorch;
		}

		public void AddPickUpPlayer(byte playerId)
			=> this.hasPlayer.Add(playerId);

		public void UpdateTimer(in float deltaTime)
		{
			this.Timer += deltaTime;
		}

		public void Remove()
		{
			foreach (var torch in this.placedTorch)
			{
				torch.Remove();
			}
		}
	}

	public enum Ops
	{
		SetTorch,
		PickUpTorch
	}

	public bool IsDirty { get; private set; } = false;

	private readonly float activeTime;
	private readonly float blackOutTime;
	private readonly float range;
	private readonly int setNum;

	private readonly HashSet<int> removeTorch = new HashSet<int>();
	private readonly Dictionary<int, int> winPlayerNum = new Dictionary<int, int>();
	private readonly Dictionary<int, TorchGroup> torchGroups = new Dictionary<int, TorchGroup>();
	private readonly Dictionary<int, int> effectPlayer = new Dictionary<int, int>();
	private int groupId = 0;

	private float blackOutTimer = 0.0f;
	private bool hasTorchHostPlayer = false;

	public WispTorchSystem(
		int setNum, float range,
		float activeTime, float blackOutTime)
	{
		this.groupId = 0;
		this.setNum = setNum;
		this.range = range;
		this.activeTime = activeTime;
		this.blackOutTime = blackOutTime;
	}

	public int CurEffectPlayerNum(Wisp wisp)
	{
		int gameControlId = replaceGameControlId(wisp.GameControlId);
		return this.effectPlayer.TryGetValue(gameControlId, out int result) ? result : 0;
	}

	public bool HasTorch(byte playerId)
		=> this.torchGroups.Values.Any(x => x.HasPlayer.Contains(playerId));

	public void SetWinPlayerNum(Wisp wisp, int num)
	{
		int gameControlId = replaceGameControlId(wisp.GameControlId);
		this.winPlayerNum[gameControlId] = num;
	}

	public bool IsWin(Wisp wisp)
	{
		int gameControlId = replaceGameControlId(wisp.GameControlId);
		return
			this.effectPlayer.TryGetValue(gameControlId, out int playerNum) &&
			this.winPlayerNum.TryGetValue(gameControlId, out int winPlayerNum)?
			playerNum >= winPlayerNum : false;
	}


	public void Deserialize(MessageReader reader, bool initialState)
	{
		int removeNum = reader.ReadPackedInt32();
		bool islocalPlayerHasTorch = false;
		byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

		for (int i = 0; i < removeNum; ++i)
		{
			int groupId = reader.ReadPackedInt32();
			if (this.torchGroups.TryGetValue(groupId, out var group))
			{
				islocalPlayerHasTorch =
					islocalPlayerHasTorch ||
					group.HasPlayer.Contains(localPlayerId);

				group.Remove();
				this.torchGroups.Remove(groupId);
			}
		}

		this.effectPlayer.Clear();
		int curStateNum = reader.ReadPackedInt32();
		for (int i = 0; i < curStateNum; ++i)
		{
			int wispId = reader.ReadPackedInt32();
			int playerNum = reader.ReadPackedInt32();
			this.effectPlayer[wispId] = playerNum;
		}


		if (removeNum > 0 && !islocalPlayerHasTorch)
		{
			this.blackOutTimer = this.blackOutTime;
			VisionComputer.Instance.SetModifier(
				VisionComputer.Modifier.WispLightOff);
		}
	}

	public void Deteriorate(float deltaTime)
	{
		if (this.blackOutTimer > 0.0f)
		{
			this.blackOutTimer -= deltaTime;
			if (this.blackOutTimer <= 0.0f)
			{
				VisionComputer.Instance.ResetModifier();
			}
		}

		if (!AmongUsClient.Instance.AmHost || this.IsDirty) { return; }

		this.removeTorch.Clear();

		foreach (var (id, group) in this.torchGroups)
		{
			group.UpdateTimer(deltaTime);

			if (group.Timer <= this.activeTime) { continue; }

			group.Remove();

			// 停電処理かつ持ってない人更新
			int playerNum = 0;
			foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
			{
				if (player.IsDead ||
					player.Disconnected ||
					group.HasPlayer.Contains(player.PlayerId)) { continue; }
				++playerNum;
			}

			int wispId = group.WispId;
			this.effectPlayer[wispId] =
				this.effectPlayer.TryGetValue(wispId, out int result) ?
				result + playerNum : playerNum;
			this.removeTorch.Add(id);
		}

		if (this.removeTorch.Count == 0) { return; }

		this.hasTorchHostPlayer = false;
		byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId; ;

		foreach (int id in this.removeTorch)
		{
			this.hasTorchHostPlayer =
				this.hasTorchHostPlayer ||
				this.torchGroups[id].HasPlayer.Contains(localPlayerId);

			this.torchGroups.Remove(id);
		}

		this.IsDirty = true;
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingStart)
		{
			foreach (var group in this.torchGroups.Values)
			{
				group.Remove();
			}
			this.torchGroups.Clear();
			VisionComputer.Instance.ResetModifier();
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		int removeNum = this.removeTorch.Count;
		writer.WritePacked(removeNum);
		foreach (int id in this.removeTorch)
		{
			writer.WritePacked(id);
		}

		writer.WritePacked(this.effectPlayer.Count);
		foreach (var (id, num) in this.effectPlayer)
		{
			writer.WritePacked(id);
			writer.WritePacked(num);
		}

		if (removeNum > 0 && !this.hasTorchHostPlayer)
		{
			this.blackOutTimer = this.blackOutTime;
			VisionComputer.Instance.SetModifier(
				VisionComputer.Modifier.WispLightOff);
		}
		this.hasTorchHostPlayer = false;
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			// ここは全員が通すようにする
			case Ops.SetTorch:
				// 灯火設置処理
				byte callWispPlayerId = msgReader.ReadByte();
				Wisp wisp = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Wisp>(callWispPlayerId);
				if (wisp != null)
				{
					setTorchGroup(wisp.GameControlId);
				}
				this.IsDirty = false;
				break;
			case Ops.PickUpTorch:
				int picupGroup = msgReader.ReadPackedInt32();
				lock (this.torchGroups)
				{
					this.torchGroups[picupGroup].AddPickUpPlayer(player.PlayerId);
				}
				this.IsDirty = false;
				break;
			default:
				return;
		}
	}

	private void setTorchGroup(int gameControlId)
	{
		gameControlId = replaceGameControlId(gameControlId);

		var torchGroup = new TorchGroup(
			this.groupId,
			gameControlId,
			this.setNum,
			this.range);

		this.torchGroups.Add(torchGroup.GroupId, torchGroup);
		++this.groupId;
	}

	private static int replaceGameControlId(int gameControlId)
		=> ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin ?
		PlayerStatistics.SameNeutralGameControlId : gameControlId;
}
