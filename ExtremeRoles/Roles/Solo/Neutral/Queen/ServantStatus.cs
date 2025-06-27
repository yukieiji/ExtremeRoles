using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

#nullable enable

public sealed class ServantStatus(byte queenRolePlayerId, QueenRole queen) : IStatusModel, IParentChainStatus
{
	public byte Parent { get; } = queenRolePlayerId;

	public void RemoveParent(byte rolePlayerId)
	{
		queen.RemoveServantPlayer(rolePlayerId);
	}
}
