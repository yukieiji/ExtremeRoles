using System.Collections.Generic;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public class SidekickStatus : IStatusModel, IParentChainStatus, IFakeImpostorStatus
{
	public byte Parent { get; }
	private readonly List<byte> sks;

    public bool IsFakeImpostor { get; }

    public SidekickStatus(byte playerId, JackalRole jackal, bool isImpostor)
    {
        this.Parent = playerId;
        this.sks = jackal.SidekickPlayerId;
        this.IsFakeImpostor = jackal.CanSeeImpostorToSidekickImpostor && isImpostor;
    }

	public void RemoveParent(byte rolePlayerId)
	{
		this.sks.Remove(rolePlayerId);
	}

	public void ClearSidekick()
	{
		this.sks.Clear();
	}
}
