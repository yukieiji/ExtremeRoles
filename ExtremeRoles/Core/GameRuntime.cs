using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Core;

public sealed class GameRuntime(IServiceScopeFactory factory) : IGameRuntime, IGameRuntimeStarter, IGameRuntimeEnder
{
	private readonly IServiceScopeFactory _factory = factory;
	private IGameContext? _current = null;


	public bool TryGetGameContext([NotNullWhen(true)] out IGameContext? context)
	{
		context = _current;
		return context is not null;
	}

	public void Start()
	{
		var scope = _factory.CreateScope();
		var newContext = new GameContext(scope);
		End();
		_current = newContext;
	}

	public void End()
	{
		var old = _current;
		_current = null;
		old?.Dispose();
	}
}
