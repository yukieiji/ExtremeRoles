using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Core.Abstract;

public interface IGameRuntime
{
	public bool TryGetGameContext([NotNullWhen(true)] out IGameContext? context);
}
