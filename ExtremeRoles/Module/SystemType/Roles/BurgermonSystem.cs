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

public enum BurgermonSpeedMode : byte
{
	Fast,
	Slow,
	None,
}

public enum BurgermonSizeMode : byte
{
	Big,
	Small,
	None,
}

public enum BurgermonRpcOps : byte
{
	FeedBurger,
}

public sealed class BurgermonSystem : IExtremeSystemType
{
	public static ExtremeSystemType Type => ExtremeSystemType.BurgermonSystem;

	private readonly Dictionary<byte, int> fedCounts = new();
	private readonly Dictionary<byte, int> pendingFedCounts = new();

	public BurgermonSpeedMode SpeedMode { get; set; } = BurgermonSpeedMode.Fast;
	public float SpeedValue { get; set; } = 0.03f; // 3%

	public BurgermonSizeMode SizeMode { get; set; } = BurgermonSizeMode.Big;
	public float SizeValue { get; set; } = 0.25f; // 25%

	public bool KeepAfterDeath { get; set; } = true;
	public bool AffectDeadBody { get; set; } = true;

	public static BurgermonSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<BurgermonSystem>(Type);

	public static bool TryGet([NotNullWhen(true)] out BurgermonSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(Type, out system);

	public int GetTotalFedCount(byte targetId)
	{
		int count = 0;
		if (this.fedCounts.TryGetValue(targetId, out int c1)) count += c1;
		if (this.pendingFedCounts.TryGetValue(targetId, out int c2)) count += c2;
		return count;
	}

	public int GetActiveFedCount(byte targetId)
	{
		return this.fedCounts.TryGetValue(targetId, out int c) ? c : 0;
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
		if (timing is ResetTiming.OnPlayer)
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
		else if (timing is ResetTiming.MeetingStart)
		{
			// Apply pending fed counts at meeting start
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
		else if (timing is ResetTiming.MeetingEnd)
		{
			ApplyAllEffects();
		}
	}

	public void ApplyAllEffects()
	{
		var speedSystem = PlayerSpeedSystem.Get();
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player.IsInValid()) continue;

			byte id = player.PlayerId;
			int count = GetActiveFedCount(id);

			// Handle death reset if KeepAfterDeath is false
			if (!this.KeepAfterDeath && player.Data != null && player.Data.IsDead)
			{
				count = 0;
			}

			// Apply speed
			float speedMod = CalculateSpeedMod(count);
			speedSystem.UpdateTo(id, speedMod);

			// Apply scale
			ApplyScaleToPlayer(player, count);
		}

		if (this.AffectDeadBody)
		{
			ApplyScaleToDeadBodies();
		}
	}

	public float CalculateSpeedMod(int count)
	{
		if (count <= 0 || this.SpeedMode == BurgermonSpeedMode.None) return 1.0f;
		return this.SpeedMode switch
		{
			BurgermonSpeedMode.Fast => 1.0f + (this.SpeedValue * count),
			BurgermonSpeedMode.Slow => Mathf.Max(0.1f, 1.0f - (this.SpeedValue * count)),
			_ => 1.0f,
		};
	}

	public float CalculateScaleFactor(int count)
	{
		if (count <= 0 || this.SizeMode == BurgermonSizeMode.None) return 1.0f;
		return this.SizeMode switch
		{
			BurgermonSizeMode.Big => 1.0f + (this.SizeValue * count),
			BurgermonSizeMode.Small => Mathf.Max(0.1f, 1.0f - (this.SizeValue * count)),
			_ => 1.0f,
		};
	}

	public void ApplyScaleToPlayer(PlayerControl player, int count)
	{
		if (player == null) return;

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
		if (!this.AffectDeadBody) return;

		var deadBodies = Object.FindObjectsOfType<DeadBody>();
		if (deadBodies == null) return;

		foreach (var body in deadBodies)
		{
			if (body == null) continue;
			byte victimId = body.ParentId;
			int count = GetActiveFedCount(victimId);
			float scaleFactor = CalculateScaleFactor(count);

			body.transform.localScale = Vector3.one * scaleFactor;
		}
	}

	public void ApplyScaleToPlayerVoteArea(PlayerVoteArea pva)
	{
		if (pva == null || pva.PlayerIcon == null) return;
		byte targetId = pva.PlayerId;
		int count = GetActiveFedCount(targetId);
		float scaleFactor = CalculateScaleFactor(count);

		pva.PlayerIcon.transform.localScale = Vector3.one * scaleFactor;
	}
}
