using System;

using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.AbilityModeSwitcher;

public struct GraphicMode<SwithEnum>(SwithEnum mode, ButtonGraphic graphic) : IAbilityMode<SwithEnum>
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
		this.Current = type;
		var mode = this.Get(this.Current);
		this.Behavior.SetGraphic(mode.Graphic);
	}
}
