using System;
using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.AbilityButton.GhostRoles
{

    public abstract class GhostRoleAbilityButtonBase : AbilityButtonBase
    {

        protected Func<bool> abilityPreCheck;
        protected Action<RPCOperator.RpcCaller> ability;
        private AbilityType abilityType;
        private Action rpcHostCallAbility;
        private bool reportAbility;

        public GhostRoleAbilityButtonBase(
            AbilityType abilityType,
            Action<RPCOperator.RpcCaller> ability,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Sprite sprite,
            Action rpcHostCallAbility = null,
            Action abilityCleanUp = null,
            Func<bool> abilityCheck = null,
            KeyCode hotkey = KeyCode.F
            ) : base(
                Helper.Translation.GetString(
                    string.Concat(abilityType.ToString(), "Button")),
                canUse, sprite,
                abilityCleanUp, abilityCheck,
                hotkey)
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
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.UseGhostRoleAbility))
                {
                    caller.WriteByte((byte)this.abilityType);
                    caller.WriteBoolean(this.reportAbility);
                    this.ability.Invoke(caller);
                }
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

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            if (localPlayer.Data == null ||
                MeetingHud.Instance ||
                ExileController.Instance ||
                !localPlayer.Data.IsDead)
            {
                SetActive(false);
                return;
            }

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            SetActive(
                localPlayer.IsKillTimerEnabled || 
                localPlayer.ForceKillTimerContinue ||
                hudManager.UseButton.isActiveAndEnabled);

            this.Button.graphic.sprite = this.ButtonSprite;
            this.Button.OverrideText(ButtonText);

            AbilityButtonUpdate();

            if (Input.GetKeyDown(this.Hotkey))
            {
                OnClickEvent();
            }

        }
    }
}
