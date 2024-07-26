using System;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;


#nullable enable

namespace ExtremeRoles.Module.Ability.ModeSwitcher;

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
		BehaviorBase behavior,
		params GraphicAndActiveTimeMode<SwithEnum>[] allMode) : base(behavior, allMode)
	{
		if (behavior is not IActivatingBehavior)
		{
			throw new ArgumentException("Can't change activating mode!!");
		}
	}

	public override void Switch(SwithEnum type)
	{
		Current = type;
		GraphicAndActiveTimeMode<SwithEnum> mode = Get(type);

		this.Behavior.SetGraphic(mode.Graphic);
		if (this.Behavior is IActivatingBehavior activatingBehavior)
		{
			activatingBehavior.ActiveTime = mode.Time;
		}
	}
}
