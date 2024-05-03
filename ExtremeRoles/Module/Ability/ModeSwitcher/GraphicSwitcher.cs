using System;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.Ability.ModeSwitcher;

public class GraphicMode<SwithEnum>(SwithEnum mode, ButtonGraphic graphic) : IAbilityMode<SwithEnum>
	where SwithEnum : struct, Enum
{
	public ButtonGraphic Graphic { get; } = graphic;
	public SwithEnum Mode { get; } = mode;
}

public class GraphicSwitcher<SwithEnum> : ModeSwitcherBase<SwithEnum, GraphicMode<SwithEnum>>
	where SwithEnum : struct, Enum
{
	public GraphicSwitcher(AbilityBehaviorBase behavior, params GraphicMode<SwithEnum>[] allMode)
		: base(behavior, allMode)
	{ }

	public override void Switch(SwithEnum type)
	{
		Current = type;

		var mode = Get(Current);
		Behavior.SetGraphic(mode.Graphic);
	}
}
