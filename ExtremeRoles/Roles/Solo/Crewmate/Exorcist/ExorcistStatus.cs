using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Exorcist;

public sealed class ExorcistStatus : IStatusModel, IDeadBodyReportOverrideStatus
{
	public bool CanReport => false;

	private readonly ExorcistRole exorcist;
	private readonly float range;
	private float awakeFakeImpTaskGage;

	public NetworkedPlayerInfo CurTarget => Player.GetDeadBodyInfo(this.range);

	public ExorcistStatus(
		ExorcistRole exorcist,
		float awakeFakeImpTaskGage,
		float range)
	{
		this.awakeFakeImpTaskGage = awakeFakeImpTaskGage;
		this.exorcist = exorcist;
		this.exorcist.FakeImpostor = awakeFakeImpTaskGage <= 0.0f;
		this.range = range;
	}

	public void FrameUpdate(PlayerControl player)
	{
		if (this.awakeFakeImpTaskGage <= 0.0f)
		{
			return;
		}
		if (Player.GetPlayerTaskGage(player) >= this.awakeFakeImpTaskGage)
		{
			rpcUpdateToFakeImpostor(player);
			UpdateToFakeImpostor();
		}
	}
	
	public void UpdateToFakeImpostor()
	{
		this.exorcist.FakeImpostor = true;
		this.awakeFakeImpTaskGage = -1.0f;
	}

	private static void rpcUpdateToFakeImpostor(PlayerControl player)
	{
		using (var op = RPCOperator.CreateCaller(
			RPCOperator.Command.ExorcistOps))
		{
			op.WriteByte((byte)ExorcistRole.RpcOpsMode.AwakeFakeImp);
			op.WriteByte(player.PlayerId);
		}
	}
}
