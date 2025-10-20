#nullable enable

namespace ExtremeRoles.Roles.API.Interface.Ability;

public interface IExiledAnimationOverrideWhenExiled
{
	public NetworkedPlayerInfo? OverideExiledTarget { get; }
	public string AnimationText { get; }
}
