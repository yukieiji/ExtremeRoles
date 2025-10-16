using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

#nullable enable

public sealed class ServantStatus : IStatusModel, IParentChainStatus, IFakeImpostorStatus
{
	public byte Parent { get; }
    public bool IsFakeImpostor { get; }
    private readonly QueenRole queen;

    public ServantStatus(byte queenRolePlayerId, QueenRole queen, SingleRoleBase baseRole)
    {
        this.Parent = queenRolePlayerId;
        this.queen = queen;

        var core = baseRole.Core;
        this.IsFakeImpostor = core.Team == ExtremeRoleType.Impostor;
    }

	public void RemoveParent(byte rolePlayerId)
	{
		queen.RemoveServantPlayer(rolePlayerId);
	}
}
