namespace ExtremeRoles.Module.Ability.Behavior.Interface;

public interface IActivatingBehavior
{
	public float ActiveTime { get; set; }
	public bool CanAbilityActiving { get; }
}
