using System;

using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.AbilityModeSwitcher
{
    public struct GraphicMode : IAbilityMode
    {
        public ButtonGraphic Graphic { get; set; }
    }

    public class GraphicSwitcher<SwithEnum> : ModeSwitcherBase<SwithEnum, GraphicMode>
        where SwithEnum : struct, Enum
    {
        public GraphicSwitcher(AbilityBehaviorBase behavior) : base(behavior)
        { }
    }
}
