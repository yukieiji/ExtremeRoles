using UnityEngine;

namespace ExtremeRoles.Module.Ability.Behavior;

public sealed class NullBehaviour : BehaviorBase
{
	public NullBehaviour() : base("", null)
	{ }

	public override void AbilityOff()
	{ }

	public override void ForceAbilityOff()
	{ }

	public override void Initialize(ActionButton button)
	{ }

	public override bool IsUse() => false;

	public override bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState)
	{
		newState = curState;
		return false;
	}

	public override AbilityState Update(AbilityState curState)
		=> curState;
}
