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
            Action abilityOff = null,
            Action forceAbilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {

            return new ExtremeAbilityButton(
                new ReusableAbilityBehavior(
                    text: Helper.Translation.GetString(
                        string.Concat(type.ToString(), "Button")),
                    img: img,
                    canUse: createGhostRoleUseFunc(canUse),
                    ability: createGhostRoleAbility(
                        type, isReport, abilityPreCheck,
                        ability, rpcHostCallAbility),
                    canActivating: canActivating,
                    abilityOff: abilityOff,
                    forceAbilityOff: forceAbilityOff),
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
            Action abilityOff = null,
            Action forceAbilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {

            return new ExtremeAbilityButton(
                new AbilityCountBehavior(
                    text: Helper.Translation.GetString(
                        string.Concat(type.ToString(), "Button")),
                    img: img,
                    canUse: createGhostRoleUseFunc(canUse),
                    ability: createGhostRoleAbility(
                        type, isReport, abilityPreCheck,
                        ability, rpcHostCallAbility),
                    canActivating: canActivating,
                    abilityOff: abilityOff,
                    forceAbilityOff: forceAbilityOff,
                    isReduceOnActive: isReduceOnActive),
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
