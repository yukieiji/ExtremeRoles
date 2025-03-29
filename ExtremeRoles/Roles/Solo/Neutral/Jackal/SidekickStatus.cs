using System.Collections.Generic;

using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public class SidekickStatus(byte playerId, JackalRole jackal) : IStatusModel, IParentChainStatus
{
	public byte Parent { get; } = playerId;
	private readonly List<byte> sks = jackal.SidekickPlayerId;

	public void RemoveParent(byte rolePlayerId)
	{
		this.sks.Remove(rolePlayerId);
	}

	public void ClearSidekick()
	{
		this.sks.Clear();
	}
}
