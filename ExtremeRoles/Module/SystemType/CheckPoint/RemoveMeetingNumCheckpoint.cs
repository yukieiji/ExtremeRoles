using Hazel;

using ExtremeRoles.Performance;
using System.Linq;
using System.Collections.Generic;

using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Module.SystemType.CheckPoint;

public sealed class RemoveMeetingNumCheckpoint : GlobalCheckpointSystem.CheckpointHandler
{
	private readonly HashSet<byte> targetPlayerId;

	public RemoveMeetingNumCheckpoint(in MessageReader reader)
	{
		int size = reader.ReadPackedInt32();
		this.targetPlayerId = new HashSet<byte>(size);
		for (int i = 0; i < size; i++)
		{
			this.targetPlayerId.Add(
				reader.ReadByte());
		}
	}

	public static void RpcCheckpoint(int num)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			GlobalCheckpointSystem.Type, ((writer) =>
			{
				var allPc = PlayerCache.AllPlayerControl.OrderBy(
					x => RandomGenerator.Instance.Next()).ToArray();

				int size = allPc.Length > num ? num : allPc.Length;
				writer.Write((byte)GlobalCheckpointSystem.CheckpointType.RemoveButton);
				writer.WritePacked(size);
				foreach (var pc in allPc[..size])
				{
					writer.Write(pc.PlayerId);
				}
			}));
	}

	public override void HandleChecked()
	{
		if (AmongUsClient.Instance.AmHost &&
			ExtremeSystemTypeManager.Instance.TryGet<MonikaMeetingNumSystem>(
				ExtremeSystemType.MonikaMeetingNumSystem, out var system))
		{
			foreach (byte id in this.targetPlayerId)
			{
				system.RpcReduceTo(id, false);
			}
		}

		var pc = PlayerControl.LocalPlayer;
		if (pc == null ||
			pc.RemainingEmergencies == 0 ||
			!this.targetPlayerId.Contains(pc.PlayerId))
		{
			return;
		}
		pc.RemainingEmergencies--;
	}
}
