using System;
using System.Collections.Generic;

using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.AbilityModeSwitcher;

public class ModeSwitcherBase<SwithEnum, ModeStruct>
	where SwithEnum : struct, Enum
	where ModeStruct : IAbilityMode<SwithEnum>
{
	protected readonly AbilityBehaviorBase Behavior = behavior;
	public SwithEnum Current { get; protected set; }

	private readonly Dictionary<SwithEnum, ModeStruct> mode = new Dictionary<SwithEnum, ModeStruct>();

	public ModeSwitcherBase(AbilityBehaviorBase behavior, params ModeStruct[] allMode)
	{
		this.Behavior = behavior;
		foreach (var mode in allMode)
		{
			this.Add(mode);
		}
	}

	public void Add(ModeStruct mode)
	{
		this.mode[mode.Mode] = mode;
	}

	public ModeStruct Get(SwithEnum type) => this.mode[type];

	public abstract void Switch(SwithEnum type);
}
