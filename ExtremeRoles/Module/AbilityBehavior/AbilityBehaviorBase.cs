using UnityEngine;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public struct ButtonGraphic
    {
        public Sprite Img { get; set; }
        public string Text { get; set; }
    }

    public abstract class AbilityBehaviorBase
    {
        public float CoolTime { get; private set; }
        public float ActiveTime { get; private set; }

        public ButtonGraphic Graphic => this.graphic;
        private ButtonGraphic graphic;

        public AbilityBehaviorBase(string text, Sprite img)
        {
            this.graphic = new ButtonGraphic()
            {
                Img = img,
                Text = text,
            };
        }

        public void SetActiveTime(float newTime)
        {
            this.ActiveTime = newTime;
        }

        public void SetCoolTime(float newTime)
        {
            this.CoolTime = newTime;
        }

        public void SetButtonImage(Sprite image)
        {
            this.graphic.Img = image;
        }

        public void SetButtonText(string text)
        {
            this.graphic.Text = text;
        }

        public abstract void Initialize(ActionButton button);

        public abstract void ForceAbilityOff();

        public abstract void AbilityOff();

        public abstract bool TryUseAbility(float timer, AbilityState curState, out AbilityState newState);

        public abstract bool IsUse();

        public abstract bool IsCanAbilityActiving();

        public abstract AbilityState Update(AbilityState curState);
    }
}
