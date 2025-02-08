using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.CheckPoint;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class GlobalCheckpointSystem : IExtremeSystemType
{
	public abstract class CheckpointHandler
	{
		public IReadOnlySet<byte> CheckedPlayer => this.playerId;

		private readonly HashSet<byte> playerId = new HashSet<byte>();

		public void AddCheckPoint(byte playerId)
			=> this.playerId.Add(playerId);

		public abstract void HandleChecked();
	}

	public enum CheckpointType
	{
		RoleAssign,
		OnemanMeeting,
		RemoveButton,
	}

	public const ExtremeSystemType Type = ExtremeSystemType.GlobalCheckpoint;

	private readonly Dictionary<CheckpointType, CheckpointHandler> checkpoints = new Dictionary<CheckpointType, CheckpointHandler>();

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		lock (this.checkpoints)
		{
			var checkPointType = (CheckpointType)msgReader.ReadByte();
			var handler = tryGetHandler(checkPointType, msgReader);

			byte playerId = player.PlayerId;
			ExtremeRolesPlugin.Logger.LogInfo(
				$"CheckPoint:{checkPointType} check to playerId:{playerId}");
			handler.AddCheckPoint(playerId);

			if (isCheckpointOk(handler))
			{
				handler.HandleChecked();
				this.checkpoints.Remove(checkPointType);
			}
		}
	}

	private CheckpointHandler tryGetHandler(in CheckpointType type, in MessageReader msgReader)
	{
		if (!this.checkpoints.TryGetValue(type, out var handler) ||
			handler is null)
		{
			handler = type switch
			{
				CheckpointType.RoleAssign => new RoleAssignCheckPoint(),
				CheckpointType.OnemanMeeting => new OnemanMeetingCheckpoint(msgReader),
				CheckpointType.RemoveButton => new RemoveMeetingNumCheckpoint(msgReader),
				_ => null,
			};

			if (handler is null)
			{
				throw new ArgumentException("InvalidType");
			}
			this.checkpoints.Add(type, handler);
		}
		return handler;
	}

	private bool isCheckpointOk(in CheckpointHandler handler)
	{
		var checkedPlayer = handler.CheckedPlayer;

		if (checkedPlayer.Count == 1)
		{
			return false;
		}

		return PlayerCache.AllPlayerControl
			.Where(
				x => x != null && x.Data != null && !x.Data.Disconnected)
			.All(
				x => checkedPlayer.Contains(x.PlayerId));
	}
}
