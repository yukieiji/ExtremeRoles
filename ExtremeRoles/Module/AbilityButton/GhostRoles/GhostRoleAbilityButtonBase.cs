using System;
using UnityEngine;

using Hazel;


namespace ExtremeRoles.Module.AbilityButton.GhostRoles
{

    public abstract class GhostRoleAbilityButtonBase : AbilityButtonBase
    {

        protected Func<bool> abilityPreCheck;
        protected Action<MessageWriter> ability;
        private GhostRoleAbilityManager.AbilityType abilityType;

        public GhostRoleAbilityButtonBase(
            GhostRoleAbilityManager.AbilityType abilityType,
            Action<MessageWriter> ability,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false) : base(
                Helper.Translation.GetString(
                    string.Concat(abilityType.ToString(), "Button")),
                canUse, sprite, positionOffset,
                abilityCleanUp, abilityCheck,
                hotkey, mirror)
        {
            this.ability = ability;
            this.abilityPreCheck = abilityPreCheck;
            this.abilityType = abilityType;
        }

        protected abstract void AbilityButtonUpdate();

        protected bool UseAbility()
        {
            if (this.abilityPreCheck())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RPCOperator.Command.UseGhostRoleAbility,
                    Hazel.SendOption.Reliable, -1);
                writer.Write((byte)this.abilityType);
                
                this.ability(writer);

                AmongUsClient.Instance.FinishRpcImmediately(writer);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool IsComSabNow()
        {
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t?.TaskType == TaskTypes.FixComms)
                {
                    return true;
                }
            }
            return false;
        }

        public sealed override void Update()
        {
            if (this.Button == null) { return; }
            if (PlayerControl.LocalPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance ||
                !PlayerControl.LocalPlayer.Data.IsDead)
            {
                SetActive(false);
                return;
            }
            SetActive(HudManager.Instance.UseButton.isActiveAndEnabled);

            this.Button.graphic.sprite = this.ButtonSprite;
            this.Button.OverrideText(ButtonText);

            if (HudManager.Instance.UseButton != null)
            {
                Vector3 pos = HudManager.Instance.UseButton.transform.localPosition;
                if (this.Mirror)
                {
                    pos = new Vector3(-pos.x, pos.y, pos.z);
                }
                this.Button.transform.localPosition = pos + PositionOffset;
            }

            AbilityButtonUpdate();

            if (Input.GetKeyDown(this.Hotkey))
            {
                OnClickEvent();
            }

        }
    }
}
