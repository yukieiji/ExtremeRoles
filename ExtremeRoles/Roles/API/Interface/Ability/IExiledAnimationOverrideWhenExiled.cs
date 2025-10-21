#nullable enable

namespace ExtremeRoles.Roles.API.Interface.Ability;

public sealed record OverideInfo(NetworkedPlayerInfo? ExiledPlayer, string AnimationText);

public interface IExiledAnimationOverrideWhenExiled
{
	public OverideInfo? OverideInfo { get; }
}
