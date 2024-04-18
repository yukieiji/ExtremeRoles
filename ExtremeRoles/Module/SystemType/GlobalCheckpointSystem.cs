using Hazel;
using System;

using System.Collections.Generic;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;
using System.Linq;
using ExtremeRoles.Module.SystemType.CheckPoint;

namespace ExtremeRoles.Module.SystemType;

public sealed class GlobalCheckpointSystem : IDirtableSystemType
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
		AssassinMeeting,
	}

	public bool IsDirty => false;

	public const ExtremeSystemType Type = ExtremeSystemType.GlobalCheckpoint;

	private readonly Dictionary<CheckpointType, CheckpointHandler> checkpoints = new Dictionary<CheckpointType, CheckpointHandler>();

	public static GlobalCheckpointSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<GlobalCheckpointSystem>(Type);

	public void Deteriorate(float deltaTime)
	{
		if (this.checkpoints.Count == 0)
		{
			return;
		}

		var removed = new HashSet<CheckpointType>();
		var invalidPlayer = CachedPlayerControl.AllPlayerControls.Where(
			x => x != null && x.Data != null && !x.Data.Disconnected);

		foreach (var (type, checkPoint) in this.checkpoints)
		{
			var checkedPlayer = checkPoint.CheckedPlayer;

			if (checkedPlayer.Count == 0 ||
				!invalidPlayer.All(
					x => checkedPlayer.Contains(x.PlayerId))
				)
			{
				continue;
			}
			checkPoint.HandleChecked();
			removed.Add(type);
		}
		foreach (var type in removed)
		{
			this.checkpoints.Remove(type);
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var checkPointType = (CheckpointType)msgReader.ReadByte();
		lock (this.checkpoints)
		{
			if (!this.checkpoints.TryGetValue(checkPointType, out var handler) ||
				handler is null)
			{
				handler = checkPointType switch
				{
					CheckpointType.RoleAssign => new RoleAssignCheckPoint(),
					CheckpointType.AssassinMeeting => new AssassinMeetingCheckpoint(msgReader),
					_ => null,
				};
				this.checkpoints.Add(checkPointType, handler);
			}
			if (handler is null)
			{
				throw new ArgumentException("InvalidType");
			}
			handler.AddCheckPoint(player.PlayerId);
		}
	}
}
