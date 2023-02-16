using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles.Module.AbilityButton.Mode
{
    public class NormalAbilityButtonMode
    {
        public string Text;
        public Sprite Img;
        public float ActiveTime;
    }

    public abstract class AbilityButtonModeBase<ButtonType, SwithEnum, ModeStruct> 
        where ButtonType : AbilityButtonBase
        where SwithEnum : struct, Enum
        where ModeStruct : NormalAbilityButtonMode
    {
        protected ButtonType Button;
        protected Dictionary<SwithEnum, ModeStruct> Mode = new Dictionary<SwithEnum, ModeStruct>();

        public AbilityButtonModeBase(ButtonType button)
        {
            this.Button = button;
            this.Mode.Clear();
        }

        public void AddMode(SwithEnum modeValue, ModeStruct mode)
        {
            this.Mode[modeValue] = mode;
        }

        public ModeStruct GetMode(SwithEnum modeValue) => this.Mode[modeValue];

        public abstract void SwithMode(SwithEnum modeValue);
    }
}
