using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using Microsoft.Extensions.DependencyInjection;

namespace ExtremeRoles.Core;


public class GameContext(IServiceScope provider) : IGameContext
{
	private readonly IServiceScope _provider = provider;

	public INomalGameRoleContainer Roles { get; } = provider.ServiceProvider.GetRequiredService<INomalGameRoleContainer>();

	public IShipGlobalOption GlobalOption => ExtremeGameModeManager.Instance.ShipOption;

	public void Dispose()
	{
		_provider.Dispose();
	}
}
