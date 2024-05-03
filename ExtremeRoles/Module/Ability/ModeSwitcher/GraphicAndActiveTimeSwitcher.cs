﻿using System;
using ExtremeRoles.Module.Ability.Behavior;

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
	{ }

	public override void Switch(SwithEnum type)
	{
		Current = type;
		GraphicAndActiveTimeMode<SwithEnum> mode = Get(type);

		Behavior.SetGraphic(mode.Graphic);
		Behavior.SetActiveTime(mode.Time);
	}
}
