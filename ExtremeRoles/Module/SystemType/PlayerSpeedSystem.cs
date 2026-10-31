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

	public static PlayerSpeedSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<PlayerSpeedSystem>(Type);

	public bool TryGetSpeed(byte targetPlayerId, out float speed)
	{
		speed = 0.0f;
		foreach (float mod in this.speedMod.Values)
		{
			speed += mod;
		}
		return isNotZero(speed);
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
			if (isNotZero(newSpeed))
			{
				this.speedMod[triggerPlayerId] = newSpeed;
			}
			else
			{
				this.speedMod.Remove(triggerPlayerId);
			}
		}
	}

	private static bool isNotZero(float value)
		=> value >= epsilon;
}
