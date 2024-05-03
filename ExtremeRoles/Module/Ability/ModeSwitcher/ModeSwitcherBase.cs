using System;
using System.Collections.Generic;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.Ability.ModeSwitcher;

public abstract class ModeSwitcherBase<SwithEnum, ModeStruct>
	where SwithEnum : struct, Enum
	where ModeStruct : IAbilityMode<SwithEnum>
{
	protected readonly AbilityBehaviorBase Behavior;
	public SwithEnum Current { get; protected set; }

	private readonly Dictionary<SwithEnum, ModeStruct> mode = new Dictionary<SwithEnum, ModeStruct>();

	public ModeSwitcherBase(AbilityBehaviorBase behavior, params ModeStruct[] allMode)
	{
		Behavior = behavior;
		foreach (var mode in allMode)
		{
			Add(mode);
		}
	}

	public void Add(ModeStruct mode)
	{
		this.mode[mode.Mode] = mode;
	}

	public ModeStruct Get(SwithEnum type) => mode[type];

	public abstract void Switch(SwithEnum type);
}
