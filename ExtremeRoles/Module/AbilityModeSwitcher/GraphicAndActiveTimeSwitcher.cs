using System;
using ExtremeRoles.Module.AbilityBehavior;

namespace ExtremeRoles.Module.AbilityModeSwitcher;

public class GraphicAndActiveTimeMode<SwithEnum>(SwithEnum mode, ButtonGraphic graphic, float time) : 
	GraphicMode<SwithEnum>(mode, graphic)
	where SwithEnum : struct, Enum
{
	public float Time { get; set; } = time;
}

public sealed class GraphicAndActiveTimeSwitcher<SwithEnum> : 
	ModeSwitcherBase<SwithEnum, GraphicAndActiveTimeMode<SwithEnum>>
	where SwithEnum : struct, Enum
{
	public GraphicAndActiveTimeSwitcher(
		AbilityBehaviorBase behavior,
		params GraphicAndActiveTimeMode<SwithEnum>[] allMode) : base(behavior, allMode)
	{ }

	public override void Switch(SwithEnum type)
	{
		this.Current = type;
		GraphicAndActiveTimeMode<SwithEnum> mode = this.Get(type);

		this.Behavior.SetGraphic(mode.Graphic);
		this.Behavior.SetActiveTime(mode.Time);
	}
}
