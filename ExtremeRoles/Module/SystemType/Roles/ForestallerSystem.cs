using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class ForestallerSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.ForestallerSystem;

	public enum Ops : byte
	{
		StartForestall,
		EndForestall,
	}

	private readonly HashSet<byte> activeForestallers = new();

	public bool IsForestallActive
	{
		get
		{
			lock (this.activeForestallers)
			{
				return this.activeForestallers.Count > 0;
			}
		}
	}

	public static void StartForestall()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Ops.StartForestall);
		});
	}

	public static void EndForestall()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Ops.EndForestall);
		});
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		lock (this.activeForestallers)
		{
			if (timing == ResetTiming.OnPlayer && resetPlayer != null)
			{
				this.activeForestallers.Remove(resetPlayer.PlayerId);
			}
			else
			{
				this.activeForestallers.Clear();
			}
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		byte playerId = player.PlayerId;

		lock (this.activeForestallers)
		{
			switch (ops)
			{
				case Ops.StartForestall:
					this.activeForestallers.Add(playerId);
					break;
				case Ops.EndForestall:
					this.activeForestallers.Remove(playerId);
					break;
			}
		}
	}
}
