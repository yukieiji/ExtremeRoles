using UnityEngine;

namespace ExtremeRoles.Module.AbilityBehavior
{
    public struct ButtonGraphic
    {
        public string Text { get; set; }
        public Sprite Img { get; set; }
    }

    public abstract class AbilityBehaviorBase
    {
        public float CoolTime { get; private set; }
        public float ActiveTime { get; private set; }

        public ButtonGraphic Graphic => this.graphic;
        private ButtonGraphic graphic;

        public AbilityBehaviorBase(string text, Sprite img)
        {
            SetGraphic(text, img);
        }

        public AbilityBehaviorBase(ButtonGraphic graphic)
        {
            SetGraphic(graphic);
        }

        public virtual void SetActiveTime(float newTime)
        {
            this.ActiveTime = newTime;
        }

        public virtual void SetCoolTime(float newTime)
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

        public void SetGraphic(string text, Sprite image)
        {
            SetGraphic(
                new ButtonGraphic
                {
                    Text = text,
                    Img = image,
                }
            );
        }

        public void SetGraphic(ButtonGraphic graphic)
        {
            this.graphic = graphic;
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
