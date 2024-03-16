using System;
using ExtremeRoles.Module.AbilityBehavior;

namespace ExtremeRoles.Module.AbilityModeSwitcher;

public struct GraphicAndActiveTimeMode<SwithEnum>(SwithEnum mode, ButtonGraphic graphic, float time) : GraphicMode(mode, graphic)
{
	public float Time { get; } = time;
}

public sealed class GraphicAndActiveTimeSwitcher<SwithEnum> : 
	GraphicSwitcher<GraphicAndActiveTimeMode<SwithEnum>>
	where SwithEnum : struct, Enum
{
	public GraphicAndActiveTimeSwitcher(
		AbilityBehaviorBase behavior,
		params GraphicAndActiveTimeMode<SwithEnum>[] allMode) : base(behavior, allMode)
	{ }

	public override void Switch(SwithEnum type)
	{
		base.Switch(type);
		GraphicAndActiveTimeMode mode = this.Get(type);
		this.Behavior.SetActiveTime(mode.Time);
	}
}
