using System.Collections.Generic;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.SystemType.SecurityDummySystem;

public sealed class SurveillanceDummySystem : ISecurityDummySystem
{
	private readonly HashSet<byte> target = new HashSet<byte>();
	private readonly PlayerShowSystem system = PlayerShowSystem.Get();

	public void Add(params byte[] players)
	{
		lock (target)
		{
			foreach (byte playerId in players)
			{
				this.target.Add(playerId);
			}
		}
	}

	private readonly Dictionary<byte, float> defaultScale = new Dictionary<byte, float>();
	private readonly Dictionary<byte, float> petScale = new Dictionary<byte, float>();

	private const float xScale = 0.0001f;

	public void Begin()
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				player.Data.Disconnected)
			{
				continue;
			}

			byte playerId = player.PlayerId;
			if (playerId == PlayerControl.LocalPlayer.PlayerId)
			{
				continue;
			}

			var curScale = player.transform.localScale;
			if (this.target.Contains(playerId))
			{
				this.system.Hide(player);
			}
		}
	}

	public void Clear()
	{
		this.Close();
		this.target.Clear();
	}

	public void Close()
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				player.Data.Disconnected)
			{
				continue;
			}

			byte playerId = player.PlayerId;
			if (playerId == PlayerControl.LocalPlayer.PlayerId)
			{
				continue;
			}

			var curScale = player.transform.localScale;
			if (this.target.Contains(playerId))
			{
				this.system.Show(player);
			}
		}
	}

	public bool PrefixUpdate(Minigame game)
	{
		return true;
	}

	public void Remove(params byte[] players)
	{
		lock (target)
		{
			foreach (byte playerId in players)
			{
				this.target.Remove(playerId);
			}
		}
	}
}
