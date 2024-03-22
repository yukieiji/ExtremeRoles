using System;
using ExtremeRoles.Module.AbilityBehavior;

namespace ExtremeRoles.Module.Interface;

public interface IAbilityMode<SwithEnum>
	where SwithEnum : struct, Enum
{
	public SwithEnum Mode { get; }
}
