using System;
using System.Collections.Generic;

using Hazel;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Module.Ability.Behavior.Interface;


namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class DelusionerCounterSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.DelusionerCounter;

	public enum Ops
	{
		Ready,
		Remove,
		ForceUse
	}

	private readonly Dictionary<byte, int> countingPlayer = new Dictionary<byte, int>();

	public bool TryGetCounter(byte playerId, out int count) =>
		this.countingPlayer.TryGetValue(playerId, out count);

	public void ReadyCounter(int counterNum)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type, (writer) =>
			{
				writer.Write((byte)Ops.Ready);
				writer.WritePacked(counterNum);
			});
	}

	public void Remove()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type, (writer) =>
			{
				writer.Write((byte)Ops.Remove);
			});
	}

	public void ReduceCounter(byte playerId, int counterNum)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type, (writer) =>
			{
				writer.Write((byte)Ops.ForceUse);
				writer.Write(playerId);
				writer.WritePacked(counterNum);
			});
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		this.countingPlayer.Clear();
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		lock(this.countingPlayer)
		{
			Ops ops = (Ops)msgReader.ReadByte();
			switch (ops)
			{
				case Ops.Ready:
					int countNum = msgReader.ReadPackedInt32();
					this.countingPlayer.Add(player.PlayerId, countNum);
					break;
				case Ops.Remove:
					this.countingPlayer.Remove(player.PlayerId);
					break;
				case Ops.ForceUse:
					byte reducePlayerId = msgReader.ReadByte();
					int resuceNum = msgReader.ReadPackedInt32();

					this.countingPlayer.Remove(reducePlayerId);

					var localPlayer = CachedPlayerControl.LocalPlayer;
					if (localPlayer.PlayerId != reducePlayerId)
					{
						return;
					}
					var delu = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Delusioner>();
					if (delu.Button?.Behavior is ICountBehavior countBehavior)
					{
						int newCount = Math.Clamp(
							countBehavior.AbilityCount - resuceNum, 0, 100);
						countBehavior.SetAbilityCount(newCount);
					}
					break;
				default:
					break;
			}
		}
	}
}
