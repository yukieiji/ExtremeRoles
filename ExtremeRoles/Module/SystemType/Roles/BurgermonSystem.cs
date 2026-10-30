using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Hazel;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public enum BurgermonRpcOps : byte
{
	FeedBurger,
}

public sealed class BurgermonSystem : IExtremeSystemType
{
	public static ExtremeSystemType Type => ExtremeSystemType.BurgermonSystem;

	private readonly Dictionary<byte, int> fedCounts = new();
	private readonly Dictionary<byte, int> pendingFedCounts = new();
	private readonly PlayerSpeedSystem speedSystem;

	public float SpeedValue { get; }
	public float SizeValue { get; }
	public bool KeepAfterDeath { get; }
	public bool AffectDeadBody { get; }

	public BurgermonSystem()
		: this(0.03f, 0.25f, true, true)
	{
	}

	public BurgermonSystem(
		float speedValue,
		float sizeValue,
		bool keepAfterDeath,
		bool affectDeadBody)
	{
		this.speedSystem = PlayerSpeedSystem.Get();
		this.SpeedValue = speedValue;
		this.SizeValue = sizeValue;
		this.KeepAfterDeath = keepAfterDeath;
		this.AffectDeadBody = affectDeadBody;
	}

	public static BurgermonSystem Get()
	{
		return ExtremeSystemTypeManager.Instance.CreateOrGet<BurgermonSystem>(Type);
	}

	public static bool TryGet([NotNullWhen(true)] out BurgermonSystem? system)
	{
		return ExtremeSystemTypeManager.Instance.TryGet(Type, out system);
	}

	public int GetTotalFedCount(byte targetId)
	{
		int count = 0;
		if (this.fedCounts.TryGetValue(targetId, out int c1))
		{
			count += c1;
		}
		if (this.pendingFedCounts.TryGetValue(targetId, out int c2))
		{
			count += c2;
		}
		return count;
	}

	public int GetActiveFedCount(byte targetId)
	{
		if (this.fedCounts.TryGetValue(targetId, out int c))
		{
			return c;
		}
		return 0;
	}

	public void FeedBurger(byte targetId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)BurgermonRpcOps.FeedBurger);
			writer.Write(targetId);
		});
	}

	public void UpdateSystem(PlayerControl player, MessageReader reader)
	{
		var ops = (BurgermonRpcOps)reader.ReadByte();
		if (ops == BurgermonRpcOps.FeedBurger)
		{
			byte targetId = reader.ReadByte();
			if (!this.pendingFedCounts.ContainsKey(targetId))
			{
				this.pendingFedCounts[targetId] = 0;
			}
			this.pendingFedCounts[targetId]++;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing == ResetTiming.OnPlayer)
		{
			if (resetPlayer == null)
			{
				this.fedCounts.Clear();
				this.pendingFedCounts.Clear();
			}
			else
			{
				this.fedCounts.Remove(resetPlayer.PlayerId);
				this.pendingFedCounts.Remove(resetPlayer.PlayerId);
			}
		}
		else if (timing == ResetTiming.MeetingStart)
		{
			foreach (var (targetId, pendingCount) in this.pendingFedCounts)
			{
				if (!this.fedCounts.ContainsKey(targetId))
				{
					this.fedCounts[targetId] = 0;
				}
				this.fedCounts[targetId] += pendingCount;
			}
			this.pendingFedCounts.Clear();

			ApplyAllEffects();
		}
		else if (timing == ResetTiming.MeetingEnd)
		{
			ApplyAllEffects();
		}
	}

	public void ApplyAllEffects()
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player.IsInValid())
			{
				continue;
			}

			byte id = player.PlayerId;
			int count = GetActiveFedCount(id);

			if (!this.KeepAfterDeath && player.Data != null && player.Data.IsDead)
			{
				count = 0;
			}

			float speedMod = CalculateSpeedMod(count);
			this.speedSystem.UpdateTo(id, speedMod);

			ApplyScaleToPlayer(player, count);
		}

		if (this.AffectDeadBody)
		{
			ApplyScaleToDeadBodies();
		}
	}

	public float CalculateSpeedMod(int count)
	{
		if (count <= 0)
		{
			return 1.0f;
		}
		float mod = 1.0f + (this.SpeedValue * count);
		return Mathf.Max(0.1f, mod);
	}

	public float CalculateScaleFactor(int count)
	{
		if (count <= 0)
		{
			return 1.0f;
		}
		float factor = 1.0f + (this.SizeValue * count);
		return Mathf.Max(0.1f, factor);
	}

	public void ApplyScaleToPlayer(PlayerControl player, int count)
	{
		if (player == null)
		{
			return;
		}

		float scaleFactor = CalculateScaleFactor(count);

		if (player.cosmetics != null)
		{
			var cosmeticsTrans = player.cosmetics.transform;
			if (cosmeticsTrans != null)
			{
				Vector3 baseScale = Vector3.one;
				if (AmongUsClient.Instance != null &&
					AmongUsClient.Instance.PlayerPrefab != null &&
					AmongUsClient.Instance.PlayerPrefab.cosmetics != null)
				{
					baseScale = AmongUsClient.Instance.PlayerPrefab.cosmetics.transform.localScale;
				}
				cosmeticsTrans.localScale = baseScale * scaleFactor;
			}

			if (player.cosmetics.currentBodySprite != null &&
				player.cosmetics.currentBodySprite.BodySprite != null)
			{
				var bodySpriteTrans = player.cosmetics.currentBodySprite.BodySprite.transform;
				if (bodySpriteTrans != null)
				{
					Vector3 baseBodyScale = Vector3.one;
					if (AmongUsClient.Instance != null &&
						AmongUsClient.Instance.PlayerPrefab != null &&
						AmongUsClient.Instance.PlayerPrefab.cosmetics != null &&
						AmongUsClient.Instance.PlayerPrefab.cosmetics.currentBodySprite != null &&
						AmongUsClient.Instance.PlayerPrefab.cosmetics.currentBodySprite.BodySprite != null)
					{
						baseBodyScale = AmongUsClient.Instance.PlayerPrefab.cosmetics.currentBodySprite.BodySprite.transform.localScale;
					}
					bodySpriteTrans.localScale = baseBodyScale * scaleFactor;
				}
			}
		}
	}

	public void ApplyScaleToDeadBodies()
	{
		if (!this.AffectDeadBody)
		{
			return;
		}

		var deadBodies = Object.FindObjectsOfType<DeadBody>();
		if (deadBodies == null)
		{
			return;
		}

		foreach (var body in deadBodies)
		{
			if (body == null)
			{
				continue;
			}
			byte victimId = body.ParentId;
			int count = GetActiveFedCount(victimId);
			float scaleFactor = CalculateScaleFactor(count);

			body.transform.localScale = Vector3.one * scaleFactor;
		}
	}

	public void ApplyScaleToPlayerVoteArea(PlayerVoteArea pva)
	{
		if (pva == null || pva.PlayerIcon == null)
		{
			return;
		}
		byte targetId = pva.PlayerId;
		int count = GetActiveFedCount(targetId);
		float scaleFactor = CalculateScaleFactor(count);

		pva.PlayerIcon.transform.localScale = Vector3.one * scaleFactor;
	}
}
