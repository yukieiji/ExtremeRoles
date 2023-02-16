using System;
using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityButton.Refacted.GhostRoles
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
            KeyCode hotkey = KeyCode.F) : base(
                sprite, Helper.Translation.GetString(
                    string.Concat(abilityType.ToString(), "Button")),
                abilityCleanUp, canUse, abilityCheck, hotkey)
        {
            this.ability = ability;
            this.abilityPreCheck = abilityPreCheck;
            this.abilityType = abilityType;
            this.rpcHostCallAbility = rpcHostCallAbility;

            this.SetButtonShow(true);
        }

        public void SetReportAbility(bool active)
        {
            this.reportAbility = active;
        }

        protected bool UseAbility()
        {
            if (this.abilityPreCheck.Invoke())
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
                    this.rpcHostCallAbility.Invoke();
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
            return PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
                CachedPlayerControl.LocalPlayer);
        }

        protected sealed override bool GetActivate()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            return
                (
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue ||
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled
                ) &&
                localPlayer.Data != null &&
                MeetingHud.Instance == null &&
                ExileController.Instance == null &&
                localPlayer.Data.IsDead;
        }
    }
}
