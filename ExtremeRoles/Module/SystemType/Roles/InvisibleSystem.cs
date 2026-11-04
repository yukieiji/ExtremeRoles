using ExtremeRoles.Helper;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.Interface;
using Hazel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;




#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class InvisibleSystem : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.InvisibleSystem;

	private const float LocalAlpha = 0.5f;
	private const float OtherAlpha = 0.0f;

	private readonly Dictionary<byte, PlayerInvisibleControl> savedAlphas = [];

	public bool IsDirty => false;

	public sealed class RendererAlphaSetter(SpriteRenderer rend)
	{
		private readonly SpriteRenderer rend = rend;
		private readonly float defaultAlpha = rend.color.a;

		public void Reset()
			=> SetAlpha(this.defaultAlpha);

		public void SetAlpha(float val)
		{
			var color = this.rend.color;
			color.a = val;
			this.rend.color = color;
		}
	}

	public sealed class PlayerInvisibleControl
	{
		private readonly List<RendererAlphaSetter> rends = [];
		private readonly TextMeshPro colorBlindText;
		private readonly byte playerId;

		public PlayerInvisibleControl(PlayerControl player)
		{
			var cosmetics = player.cosmetics;
			add(cosmetics.currentBodySprite.BodySprite);
			if (cosmetics.currentBodySprite.LongModeParts != null)
			{
				foreach (var part in cosmetics.currentBodySprite.LongModeParts)
				{
					add(part);
				}
			}

			if (cosmetics.skin != null)
			{
				add(cosmetics.skin.layer);
			}

			if (cosmetics.hat != null)
			{
				add(cosmetics.hat.BackLayer);
				add(cosmetics.hat.FrontLayer);
				add(cosmetics.hat.Parent);
			}

			if (cosmetics.visor != null)
			{
				add(cosmetics.visor.Image);
			}

			if (cosmetics.CurrentPet != null)
			{
				foreach (var rend in cosmetics.CurrentPet.renderers)
				{
					add(rend);
				}
				foreach (var rend in cosmetics.CurrentPet.shadows)
				{
					add(rend);
				}
				add(cosmetics.PettingHand.HandSprite);
				add(cosmetics.PettingHand.hearts);
			}
			this.colorBlindText = cosmetics.colorBlindText;
			this.playerId = player.PlayerId;
		}

		private void add(SpriteRenderer rend)
		{
			var setter = new RendererAlphaSetter(rend);
			this.rends.Add(setter);
		}

		public void Reset()
		{
			foreach (var rend in this.rends)
			{
				rend.Reset();
			}

			this.colorBlindText.enabled = true;
		}

		public void Invisible()
		{
			var local = PlayerControl.LocalPlayer;
			bool isLocal = local != null && local.PlayerId == this.playerId;
			float val = isLocal ? LocalAlpha : OtherAlpha;
			this.colorBlindText.enabled = isLocal;

			foreach (var rend in this.rends)
			{
				rend.SetAlpha(val);
			}
		}
	}

	public static InvisibleSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<InvisibleSystem>(Type);

	public static bool TryGet([NotNullWhen(true)] out InvisibleSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(Type, out system);

	public void SetInvisible(byte playerId, bool isInvisible)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write(playerId);
			writer.Write(isInvisible);
		});
	}

	public Color UpdateNameText(PlayerControl target, Color color)
	{
		if (this.savedAlphas.Count == 0)
		{
			return color;
		}

		var local = PlayerControl.LocalPlayer;
		if (this.savedAlphas.ContainsKey(target.PlayerId) &&
			local.PlayerId != target.PlayerId)
		{
			color.a = OtherAlpha;
			return color;
		}
		return color;
	}

	public void Deteriorate(float _)
	{
		foreach (var playerInvisibleControl in this.savedAlphas.Values)
		{
			playerInvisibleControl.Invisible();
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart &&
			this.savedAlphas.Count > 0)
		{
			lock (this.savedAlphas)
			{
				foreach (var playerInvisibleControl in this.savedAlphas.Values)
				{
					playerInvisibleControl.Reset();
				}
				this.savedAlphas.Clear();
				EventManager.Instance.Invoke(ModEvent.VisualUpdate);
			}
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte targetPlayerId = msgReader.ReadByte();
		bool isInvisible = msgReader.ReadBoolean();

		if (isInvisible)
		{
			setInvisibleAlpha(targetPlayerId);
		}
		else
		{
			restoreAlpha(targetPlayerId);
		}
		EventManager.Instance.Invoke(ModEvent.VisualUpdate);
	}

	private void setInvisibleAlpha(byte playerId)
	{
		lock (this.savedAlphas)
		{
			var target = Player.GetPlayerControlById(playerId);
			if (target != null && 
				target.cosmetics != null && 
				!this.savedAlphas.ContainsKey(target.PlayerId))
			{
				this.savedAlphas[target.PlayerId] = new PlayerInvisibleControl(target);
			}
		}
	}

	private void restoreAlpha(byte playerId)
	{
		lock (this.savedAlphas)
		{
			if (this.savedAlphas.TryGetValue(playerId, out var saved))
			{
				saved.Reset();
				this.savedAlphas.Remove(playerId);
			}
		}
	}

	public void MarkClean()
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
