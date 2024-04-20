using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.SystemType.CheckPoint;

public sealed class RoleAssignCheckPoint : GlobalCheckpointSystem.CheckpointHandler
{
	public static void RpcCheckpoint()
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			GlobalCheckpointSystem.Type, (writer) =>
			{
				writer.Write((byte)GlobalCheckpointSystem.CheckpointType.RoleAssign);
			});
	}
	public override void HandleChecked()
	{
		RoleAssignState.Instance.SwitchToReady();
	}
}
