namespace ExtremeRoles.Module.Ability.Behavior.Interface;

public interface IReclickBehavior
{
	public const float Offset = 0.25f;

	protected static bool CanReClick(in float timer, in float activeTime)
		=> timer <= activeTime - Offset;
}
