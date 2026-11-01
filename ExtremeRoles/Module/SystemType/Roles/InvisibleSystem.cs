using System.Collections.Generic;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using Hazel;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class InvisibleSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.InvisibleSystem;

	private const float LocalAlpha = 0.5f;
	private const float OtherAlpha = 0.0f;

	private readonly HashSet<byte> activeInvisibles = [];
	private readonly Dictionary<byte, Dictionary<SpriteRenderer, float>> savedAlphas = [];

	public static InvisibleSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<InvisibleSystem>(Type);

	public bool IsInvisible(byte playerId)
		=> this.activeInvisibles.Contains(playerId);

	public void SetInvisible(byte playerId, bool isInvisible)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write(playerId);
			writer.Write(isInvisible);
		});
	}

	public void ApplyState(byte playerId, bool isInvisible)
	{
		lock (this.activeInvisibles)
		{
			if (isInvisible)
			{
				if (this.activeInvisibles.Add(playerId))
				{
					setInvisibleAlpha(playerId);
				}
			}
			else
			{
				if (this.activeInvisibles.Remove(playerId))
				{
					restoreAlpha(playerId);
				}
			}
		}
	}

	public void Update()
	{
		if (this.activeInvisibles.Count == 0)
		{
			return;
		}

		var local = PlayerControl.LocalPlayer;
		byte localId = local != null ? local.PlayerId : byte.MaxValue;

		lock (this.activeInvisibles)
		{
			foreach (byte playerId in this.activeInvisibles)
			{
				var target = Player.GetPlayerControlById(playerId);
				if (target == null || target.cosmetics == null)
				{
					continue;
				}

				if (playerId != localId)
				{
					if (target.cosmetics.nameText != null)
					{
						var c = target.cosmetics.nameText.color;
						if (c.a != OtherAlpha)
						{
							c.a = OtherAlpha;
							target.cosmetics.nameText.color = c;
						}
					}
				}
			}
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.OnPlayer or ResetTiming.MeetingStart)
		{
			lock (this.activeInvisibles)
			{
				var activeIds = new List<byte>(this.activeInvisibles);
				foreach (byte playerId in activeIds)
				{
					restoreAlpha(playerId);
				}
				this.activeInvisibles.Clear();
				this.savedAlphas.Clear();
			}
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte targetPlayerId = msgReader.ReadByte();
		bool isInvisible = msgReader.ReadBoolean();
		ApplyState(targetPlayerId, isInvisible);
	}

	private void setInvisibleAlpha(byte playerId)
	{
		var target = Player.GetPlayerControlById(playerId);
		if (target == null || target.cosmetics == null)
		{
			return;
		}

		var local = PlayerControl.LocalPlayer;
		bool isLocal = local != null && local.PlayerId == playerId;
		float targetAlpha = isLocal ? LocalAlpha : OtherAlpha;

		var saved = new Dictionary<SpriteRenderer, float>();

		void applyRendererAlpha(SpriteRenderer? rend)
		{
			if (rend == null)
			{
				return;
			}
			var col = rend.color;
			saved[rend] = col.a;
			col.a = targetAlpha;
			rend.color = col;
		}

		var cosmetics = target.cosmetics;
		if (cosmetics.currentBodySprite != null)
		{
			applyRendererAlpha(cosmetics.currentBodySprite.BodySprite);
			if (cosmetics.currentBodySprite.LongModeParts != null)
			{
				foreach (var part in cosmetics.currentBodySprite.LongModeParts)
				{
					applyRendererAlpha(part);
				}
			}
		}

		if (cosmetics.skin != null)
		{
			applyRendererAlpha(cosmetics.skin.layer);
		}

		if (cosmetics.hat != null)
		{
			applyRendererAlpha(cosmetics.hat.BackLayer);
			applyRendererAlpha(cosmetics.hat.FrontLayer);
		}

		if (cosmetics.visor != null)
		{
			applyRendererAlpha(cosmetics.visor.Image);
		}

		if (cosmetics.CurrentPet != null)
		{
			var petRend = cosmetics.CurrentPet.GetComponent<SpriteRenderer>();
			applyRendererAlpha(petRend);
		}

		this.savedAlphas[playerId] = saved;

		if (!isLocal && cosmetics.nameText != null)
		{
			var nameCol = cosmetics.nameText.color;
			nameCol.a = OtherAlpha;
			cosmetics.nameText.color = nameCol;
		}
	}

	private void restoreAlpha(byte playerId)
	{
		var target = Player.GetPlayerControlById(playerId);

		if (this.savedAlphas.TryGetValue(playerId, out var saved))
		{
			foreach (var kvp in saved)
			{
				var rend = kvp.Key;
				if (rend != null)
				{
					var col = rend.color;
					col.a = kvp.Value;
					rend.color = col;
				}
			}
			this.savedAlphas.Remove(playerId);
		}

		if (target != null && target.cosmetics != null && target.cosmetics.nameText != null)
		{
			var nameCol = target.cosmetics.nameText.color;
			nameCol.a = 1.0f;
			target.cosmetics.nameText.color = nameCol;
		}
	}
}
