using ExtremeRoles.Module.Interface;
using Hazel;
using System.Collections.Generic;

namespace ExtremeRoles.Module.SystemType;

public class PlayerSpeedSystem : IExtremeSystemType
{
	private readonly Dictionary<byte, float> speedMod = [];

	public const ExtremeSystemType Type = ExtremeSystemType.SpeedSystem;
	private const float epsilon = 0.001f;

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
	}

	public static bool TryGetSpeed(byte targetPlayerId, out float speed)
	{
		speed = 0.0f;
		if (!ExtremeSystemTypeManager.Instance.TryGet<PlayerSpeedSystem>(Type, out var system))
		{
			return false;
		}

		foreach (float mod in system.speedMod.Values)
		{
			speed += mod;
		}
		return isCloseToZero(speed);
	}

	public void UpdateTo(byte target, float speed)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, x =>
		{
			x.Write(target);
			x.Write(speed);
		});
	}

	public void UpdateSelfSpeed(float speed)
	{
		updateSpeed(PlayerControl.LocalPlayer.PlayerId, speed);
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte triggerPlayerId = player.PlayerId;
		byte targetPlayerId = msgReader.ReadByte();
		float speed = msgReader.ReadSingle();

		if (PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.PlayerId != targetPlayerId)
		{
			return;
		}
		updateSpeed(triggerPlayerId, speed);
	}

	private void updateSpeed(byte triggerPlayerId, float newSpeed)
	{
		lock (this.speedMod)
		{
			if (isCloseToZero(newSpeed))
			{
				this.speedMod.Remove(triggerPlayerId);
			}
			else
			{
				this.speedMod[triggerPlayerId] = newSpeed;
			}
		}
	}

	private static bool isCloseToZero(float value)
		=> value < epsilon;
}
