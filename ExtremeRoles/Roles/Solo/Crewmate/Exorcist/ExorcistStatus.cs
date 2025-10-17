using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Exorcist;

public sealed class ExorcistStatus : IStatusModel, IDeadBodyReportOverrideStatus, IFakeImpostorStatus, IRoleFakeIntro
{
	public ExtremeRoleType FakeTeam => this.IsFakeImpostor ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate;

	public bool CanReport => false;

	private readonly float range;
	private float awakeFakeImpTaskGage;

	public bool IsFakeImpostor { get; private set; }
	public NetworkedPlayerInfo CurTarget => Player.GetDeadBodyInfo(this.range);

	public ExorcistStatus(
		float awakeFakeImpTaskGage,
		float range)
	{
		this.awakeFakeImpTaskGage = awakeFakeImpTaskGage;
		this.IsFakeImpostor = awakeFakeImpTaskGage <= 0.0f;
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
		this.IsFakeImpostor = true;
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
