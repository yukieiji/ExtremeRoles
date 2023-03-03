using System;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.AbilityModeSwitcher
{
    public struct GraphicAndActiveTimeMode : IAbilityMode
    {
        public ButtonGraphic Graphic { get; set; }
        public float Time { get; set; }
    }

    public class GraphicAndActiveTimeSwitcher<SwithEnum> : 
        ModeSwitcherBase<SwithEnum, GraphicAndActiveTimeMode>
        where SwithEnum : struct, Enum
    {
        public GraphicAndActiveTimeSwitcher(AbilityBehaviorBase behavior) : base(behavior)
        { }

        public override void Switch(SwithEnum type)
        {
            base.Switch(type);
            GraphicAndActiveTimeMode mode = this.Mode[type];
            this.Behavior.SetActiveTime(mode.Time);
        }
    }
}
