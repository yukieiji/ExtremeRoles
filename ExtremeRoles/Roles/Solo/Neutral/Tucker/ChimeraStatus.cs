
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Tucker;

public sealed class ChimeraStatus(NetworkedPlayerInfo? tuckerPlayer, ChimeraRole chimera) : IStatusModel, IParentChainStatus
{
	public byte Parent { get; } = tuckerPlayer == null ? byte.MinValue : tuckerPlayer.PlayerId;
	private readonly ChimeraRole chimera = chimera;

	public bool IsTuckerDead { get; set; } = tuckerPlayer.IsInValid();

	public void RemoveParent(byte rolePlayerId)
	{
		if (!ExtremeRoleManager.TryGetSafeCastedRole<TuckerRole>(Parent, out var tucker))
		{
			return;
		}
		tucker.OnResetChimera(rolePlayerId, chimera.KillCoolTime);
	}
}
