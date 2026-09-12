using ExtremeRoles.GameMode.Option.ShipGlobal;
using System;

namespace ExtremeRoles.Core.Abstract;

public interface IGameContext : IDisposable
{
	public INomalGameRoleContainer Roles { get; }
	public IShipGlobalOption GlobalOption { get; }
}
