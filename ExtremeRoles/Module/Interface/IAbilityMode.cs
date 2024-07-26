using System;
using ExtremeRoles.Module.Ability.Behavior;

namespace ExtremeRoles.Module.Interface;

public interface IAbilityMode<SwithEnum>
	where SwithEnum : struct, Enum
{
	public SwithEnum Mode { get; }
}
