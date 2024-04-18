using Hazel;

using ExtremeRoles.Performance;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.SystemType.CheckPoint;

public sealed class AssassinMeetingCheckpoint : GlobalCheckpointSystem.CheckpointHandler
{
	private readonly PlayerControl rolePlayer;

	public AssassinMeetingCheckpoint(in MessageReader reader)
	{
		byte rolePlayerId = reader.ReadByte();
		this.rolePlayer = Player.GetPlayerControlById(rolePlayerId);
	}

	public static void RpcCheckpoint(byte rolePlayrId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			GlobalCheckpointSystem.Type, (writer) =>
			{
				writer.Write((byte)GlobalCheckpointSystem.CheckpointType.AssassinMeeting);
				writer.Write(rolePlayrId);
			});
	}

	public override void HandleChecked()
	{
		if (this.rolePlayer == null) { return; }
		ExtremeRolesPlugin.Logger.LogInfo("StartMeeting");
		MeetingRoomManager.Instance.AssignSelf(this.rolePlayer, null);
		FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(
			this.rolePlayer);
		this.rolePlayer.RpcStartMeeting(null);
	}
}
