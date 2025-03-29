
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Tucker;

public sealed class ChimeraStatus(NetworkedPlayerInfo tuckerPlayer, ChimeraRole chimera) : IStatusModel, IParentChainStatus
{
	public byte Parent { get; } = tuckerPlayer.PlayerId;
	private readonly ChimeraRole chimera = chimera;

	public void RemoveParent(byte rolePlayerId)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedRole<TuckerRole>(Parent, out var tucker))
		{
			return;
		}
		tucker.OnResetChimera(rolePlayerId, chimera.KillCoolTime);
	}
}
