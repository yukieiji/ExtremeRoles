using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

using Hazel;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class PlayerShowSystem : IExtremeSystemType
{
	private const float xScale = 0.0001f;
	private readonly Dictionary<byte, float> defaultScale = new Dictionary<byte, float>();
	private readonly Dictionary<byte, float> petScale = new Dictionary<byte, float>();

	public static PlayerShowSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<PlayerShowSystem>(ExtremeSystemType.PlayerShowSystem);

	public static bool TryGet([NotNullWhen(true)] out PlayerShowSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.PlayerShowSystem, out system);

	public static bool TryGetScale(byte playerId, out float scale)
	{
		if (!TryGet(out var system))
		{
			scale = float.MaxValue;
			return false;
		}
		return system.defaultScale.TryGetValue(playerId, out scale);
	}

	public bool IsHide(PlayerControl target)
		=> target.transform.localScale.x == xScale;

	public void Hide(bool isIgnoreLocal=true)
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				player.Data.Disconnected)
			{
				continue;
			}

			if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && isIgnoreLocal)
			{
				continue;
			}
			Hide(player);
		}
	}

	public void Hide(PlayerControl target)
	{
		var curScale = target.transform.localScale;
		if (curScale.x == xScale)
		{
			return;
		}
		byte playerId = target.PlayerId;
		this.defaultScale[playerId] = curScale.x;
		target.transform.localScale = new Vector3(xScale, curScale.y, curScale.z);
		if (target.cosmetics != null && target.cosmetics.CurrentPet != null)
		{
			var petScale = target.cosmetics.CurrentPet.transform.localScale;
			this.petScale[playerId] = target.cosmetics.CurrentPet.transform.localScale.x;
			target.cosmetics.CurrentPet.transform.localScale = new Vector3(xScale, petScale.y, petScale.z);
		}
	}
	public void Show(bool isIgnoreLocal=true)
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				player.Data.Disconnected)
			{
				continue;
			}

			if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && isIgnoreLocal)
			{
				continue;
			}
			Show(player);
		}
	}

	public void Show(PlayerControl target)
	{
		byte playerId = target.PlayerId;
		if (!this.defaultScale.TryGetValue(playerId, out float scale))
		{
			return;
		}

		var curScale = target.transform.localScale;

		target.transform.localScale = new Vector3(scale, curScale.y, curScale.z);
		if (target.cosmetics != null &&
			target.cosmetics.CurrentPet != null &&
			this.petScale.TryGetValue(playerId, out float petDefaultScale))
		{
			var petScale = target.cosmetics.CurrentPet.transform.localScale;
			target.cosmetics.CurrentPet.transform.localScale = new Vector3(petDefaultScale, petScale.y, petScale.z);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.OnPlayer or ResetTiming.MeetingStart)
		{
			Show();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}
}
