using ExtremeRoles.GameMode.Option.ShipGlobal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Core.Abstract;

public interface IGameContext : IDisposable
{
	public INomalGameRoleContainer Roles { get; }
	public IShipGlobalOption GlobalOption { get; }
}
