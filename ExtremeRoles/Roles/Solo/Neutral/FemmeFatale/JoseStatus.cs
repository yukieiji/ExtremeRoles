using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

#nullable enable

public sealed class JoseStatus : IStatusModel, IParentChainStatus, IFakeImpostorStatus
{
	public byte Parent { get; }
	public bool IsFakeImpostor { get; }
	private readonly FemmeFataleRole femmeFatale;

	public JoseStatus(byte femmeFatalePlayerId, FemmeFataleRole femmeFatale, SingleRoleBase baseRole)
	{
		this.Parent = femmeFatalePlayerId;
		this.femmeFatale = femmeFatale;

		var core = baseRole.Core;
		this.IsFakeImpostor = core.Team is ExtremeRoleType.Impostor;
	}

	public void RemoveParent(byte rolePlayerId)
	{
		femmeFatale.RemoveJosePlayer(rolePlayerId);
	}
}
