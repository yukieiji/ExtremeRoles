using System;
using System.Collections.Generic;

using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.AbilityModeSwitcher
{
    public struct GraphicMode : IAbilityMode
    {
        public ButtonGraphic Graphic { get; set; }
    }

    public class GraphicSwitcher<SwithEnum, ModeStruct>
        where SwithEnum : struct, Enum
        where ModeStruct : IAbilityMode
    {
        protected AbilityBehaviorBase Behavior;
        protected Dictionary<SwithEnum, ModeStruct> Mode = new Dictionary<SwithEnum, ModeStruct>();

        public GraphicSwitcher(AbilityBehaviorBase behavior)
        {
            this.Behavior = behavior;
        }

        public void Add(SwithEnum type, ModeStruct mode)
        {
            this.Mode[type] = mode;
        }

        public ModeStruct Get(SwithEnum type) => this.Mode[type];

        public virtual void Switch(SwithEnum type)
        {
            ModeStruct mode = this.Mode[type];
            this.Behavior.SetGraphic(mode.Graphic);
        }
    }
}
