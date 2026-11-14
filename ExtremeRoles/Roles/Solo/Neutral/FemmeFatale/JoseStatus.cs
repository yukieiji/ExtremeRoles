using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

#nullable enable

public sealed class JoseStatus(byte femmeFatalePlayerId, FemmeFataleRole femmeFatale) : IStatusModel, IParentChainStatus
{
	public byte Parent { get; } = femmeFatalePlayerId;
	private readonly FemmeFataleRole femmeFatale = femmeFatale;

	public void RemoveParent(byte rolePlayerId)
	{
		femmeFatale.RemoveJosePlayer(rolePlayerId);
	}
}
