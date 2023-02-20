using System;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityFactory
{
    public static class GhostRoleAbilityFactory
    {
        public static ExtremeAbilityButton CreateReusableAbility(
            AbilityType type,
            Sprite img,
            bool isReport,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Action<RPCOperator.RpcCaller> ability,
            Action rpcHostCallAbility,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {

            return new ExtremeAbilityButton(
                new ReusableAbilityBehavior(
                    Helper.Translation.GetString(
                        string.Concat(type.ToString(), "Button")),
                    img, createGhostRoleUseFunc(canUse),
                    createGhostRoleAbility(
                        type, isReport, abilityPreCheck,
                        ability, rpcHostCallAbility),
                    canActivating,
                    abilityCleanUp,
                    forceAbilityOff),
                new GhostRoleButtonActivator(),
                hotKey
            );
        }

        public static ExtremeAbilityButton CreateCountAbility(
            AbilityType type,
            Sprite img,
            bool isReport,
            Func<bool> abilityPreCheck,
            Func<bool> canUse,
            Action<RPCOperator.RpcCaller> ability,
            Action rpcHostCallAbility,
            bool isReduceOnActive = false,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {

            return new ExtremeAbilityButton(
                new AbilityCountBehavior(
                    Helper.Translation.GetString(
                        string.Concat(type.ToString(), "Button")),
                    img, createGhostRoleUseFunc(canUse),
                    createGhostRoleAbility(
                        type, isReport, abilityPreCheck,
                        ability, rpcHostCallAbility),
                    canActivating,
                    abilityCleanUp,
                    forceAbilityOff,
                    isReduceOnActive),
                new GhostRoleButtonActivator(),
                hotKey
            );
        }

        private static Func<bool> createGhostRoleUseFunc(Func<bool> isUse)
        {
            return () => 
                isUse.Invoke() && 
                !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
                    CachedPlayerControl.LocalPlayer);
        }

        private static Func<bool> createGhostRoleAbility(
            AbilityType type, bool isReportAbility,
            Func<bool> abilityPreCheck,
            Action<RPCOperator.RpcCaller> ability,
            Action rpcHostCallAbility)
        {
            return () =>
            {
                if (!abilityPreCheck.Invoke())
                {
                    return false;
                }

                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.UseGhostRoleAbility))
                {
                    caller.WriteByte((byte)type);
                    caller.WriteBoolean(isReportAbility);
                    ability.Invoke(caller);
                }

                rpcHostCallAbility?.Invoke();

                if (isReportAbility)
                {
                    ExtremeRolesPlugin.ShipState.AddGhostRoleAbilityReport(type);
                }

                return true;
            };
        }
    }
}
