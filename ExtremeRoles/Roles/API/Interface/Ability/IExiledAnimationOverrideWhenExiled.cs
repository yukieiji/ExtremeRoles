#nullable enable

namespace ExtremeRoles.Roles.API.Interface.Ability;

public sealed record OverrideInfo(NetworkedPlayerInfo? ExiledPlayer, string AnimationText);

public interface IExiledAnimationOverrideWhenExiled
{
	public OverrideInfo? OverrideInfo { get; }
}
