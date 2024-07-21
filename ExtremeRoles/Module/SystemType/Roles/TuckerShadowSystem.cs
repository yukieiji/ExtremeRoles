using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class TuckerShadowSystem(
	float shadowRand,
	float shadowTimer,
	float shadowRemoveKillCool,
	bool isReduceInitKillCoolOnRemove) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.TuckerShadow;

	public enum Ops : byte
	{
		Remove,
		ChimeraRevive
	}

	private sealed record ShadowInfo(byte PlayerId, Vector2 Pos)
	{
		public static ShadowInfo? Create(byte playerId, float rand)
		{
			var player = Player.GetPlayerControlById(playerId);
			if (player == null)
			{
				return null;
			}
			var pos = player.GetTruePosition();
			pos += (Random.insideUnitCircle * rand);
			return new ShadowInfo(playerId, pos);
		}
		public void Serialize(in MessageWriter writer)
		{
			writer.Write(this.PlayerId);
			writer.Write(Pos.x);
			writer.Write(Pos.y);
		}
		public static ShadowInfo Deserialize(in MessageReader reader)
		{
			byte playerId = reader.ReadByte();
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			return new ShadowInfo(playerId, new Vector2(x, y));
		}
	}

	private readonly bool isReduceInitKillCoolOnRemove = isReduceInitKillCoolOnRemove;
	private readonly float shadowRemoveKillCool = shadowRemoveKillCool;
	private readonly float shadowTimer = shadowTimer;
	private readonly float shadowRand = shadowRand;

	private readonly Dictionary<byte, Dictionary<int, SpriteRenderer>> placedShadow = new Dictionary<byte, Dictionary<int, SpriteRenderer>>();
	private Dictionary<byte, float> timers = new Dictionary<byte, float>();

	private readonly List<ShadowInfo> scheduledShadow = new List<ShadowInfo>();
	private readonly List<ShadowInfo> cache = new List<ShadowInfo>();

	private int id = 0;

	public bool IsDirty { get; private set; }

	public void Deteriorate(float deltaTime)
	{
		if (this.timers.Count == 0 ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			return;
		}
		var newTimer = new Dictionary<byte, float>(this.timers.Count);
		foreach (var (playerId, timer) in this.timers)
		{
			float newTime = timer - deltaTime;
			if (newTime < 0)
			{
				newTime = this.shadowTimer;

				var shadow = ShadowInfo.Create(playerId, this.shadowRand);
				if (shadow != null)
				{
					this.cache.Add(shadow);
					this.IsDirty = true;
				}
			}
			newTimer[playerId] = newTime;
		}
		this.timers = newTimer;
	}

	public void Enable(byte playerId)
	{
		this.timers.TryAdd(playerId, this.shadowTimer);
	}
	public void Disable(byte playerId)
	{
		this.timers.Remove(playerId);
		this.scheduledShadow.RemoveAll(x => x.PlayerId == playerId);
	}

	public bool TryGetClosedShadowId(PlayerControl player, float range, out int id)
	{
		id = int.MaxValue;
		if (!this.placedShadow.TryGetValue(player.PlayerId, out var playerShadow))
		{
			return false;
		}
		Vector2 pos = player.transform.position;
		float closestDist = float.MaxValue;

		foreach (var (shadowId, shadow) in playerShadow)
		{
			Vector2 targetPos = shadow.transform.position;

			float checkDist = Vector2.Distance(pos, targetPos);

			if (checkDist < closestDist)
			{
				id = shadowId;
			}
		}
		return closestDist != float.MaxValue;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			this.scheduledShadow.Add(
				ShadowInfo.Deserialize(reader));
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.MeetingEnd)
		{
			return;
		}
		foreach (var shadowInfo in this.scheduledShadow)
		{
			byte playerId = shadowInfo.PlayerId;
			if (!this.placedShadow.TryGetValue(playerId, out var shadow) ||
				shadow is null)
			{
				shadow = new Dictionary<int, SpriteRenderer>();
				this.placedShadow[playerId] = shadow;
			}
			var shdowObj = new GameObject($"Shadow_{id}");
			var rend = shdowObj.AddComponent<SpriteRenderer>();
			rend.sprite = UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.TestButton);
			if (ExtremeRoleManager.TryGetSafeCastedLocalRole<Tucker>(out var tucker))
			{
				rend.color = tucker.GetNameColor();
			}
			shadow.Add(id, rend);
			id++;
		}
		this.scheduledShadow.Clear();
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		switch ((Ops)msgReader.ReadByte())
		{
			case Ops.Remove:
				byte playerId = msgReader.ReadByte();
				int id = msgReader.ReadPackedInt32();
				if (this.placedShadow.TryGetValue(playerId, out var playerShadow) &&
					playerShadow is not null &&
					playerShadow.TryGetValue(id, out var rend) &&
					rend != null)
				{
					rend.enabled = false;
					Object.Destroy(rend.gameObject);
					playerShadow.Remove(id);
				}
				if (ExtremeRoleManager.TryGetSafeCastedLocalRole<Chimera>(out var chimera))
				{
					chimera.OnRemoveShadow(
						playerId, this.shadowRemoveKillCool,
						this.isReduceInitKillCoolOnRemove);
				}
				break;
			case Ops.ChimeraRevive:
				byte parentPlayerId = msgReader.ReadByte();
				var newInfo = ShadowInfo.Create(parentPlayerId, this.shadowRand);
				if (newInfo is null)
				{
					return;
				}
				this.scheduledShadow.Add(newInfo);
				break;
			default:
				break;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		if (this.cache.Count == 0)
		{
			this.IsDirty = initialState;
			return;
		}
		writer.WritePacked(this.cache.Count);
		foreach (var shadow in this.cache)
		{
			shadow.Serialize(writer);
		}
		this.cache.Clear();
		this.IsDirty = initialState;
	}
}
