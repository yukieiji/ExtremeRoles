using System;
using UnityEngine;

using Hazel;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.AbilityButton.GhostRoles
{

    public abstract class GhostRoleAbilityButtonBase : AbilityButtonBase
    {

        protected Func<bool> abilityPreCheck;
        protected Action<MessageWriter> ability;
        private AbilityType abilityType;
        private Action rpcHostCallAbility;
        private bool reportAbility;

        public GhostRoleAbilityButtonBase(
            AbilityType abilityType,
            Action<MessageWriter> ability,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Sprite sprite,
            Vector3 positionOffset,
            Action rpcHostCallAbility = null,
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
            this.rpcHostCallAbility = rpcHostCallAbility;
        }

        public void SetReportAbility(bool active)
        {
            this.reportAbility = active;
        }

        protected abstract void AbilityButtonUpdate();

        protected bool UseAbility()
        {
            if (this.abilityPreCheck())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                    (byte)RPCOperator.Command.UseGhostRoleAbility,
                    Hazel.SendOption.Reliable, -1);
                writer.Write((byte)this.abilityType);
                writer.Write(this.reportAbility);
                
                this.ability(writer);

                AmongUsClient.Instance.FinishRpcImmediately(writer);
                if (this.rpcHostCallAbility != null)
                {
                    this.rpcHostCallAbility();
                }
                if (this.reportAbility)
                {
                    ExtremeRolesPlugin.ShipState.AddGhostRoleAbilityReport(
                        this.abilityType);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool IsComSabNow()
        {
            foreach (PlayerTask t in 
                CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
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
            if (CachedPlayerControl.LocalPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance ||
                !CachedPlayerControl.LocalPlayer.Data.IsDead)
            {
                SetActive(false);
                return;
            }

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            SetActive(hudManager.UseButton.isActiveAndEnabled);

            this.Button.graphic.sprite = this.ButtonSprite;
            this.Button.OverrideText(ButtonText);

            if (hudManager.UseButton != null)
            {
                Vector3 pos = hudManager.UseButton.transform.localPosition;
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
