using System;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.AbilityFactory
{
    public static class RoleAbilityFactory
    {
        public static ExtremeAbilityButton CreateChargableAbility(
            string textKey,
            Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new ChargableAbilityBehavior(
                    text: Helper.Translation.GetString(textKey),
                    img: img,
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityCleanUp,
                    forceAbilityOff: forceAbilityOff),
                new RoleButtonActivator(),
                hotKey
            );
        }

        public static ExtremeAbilityButton CreateCountAbility(
            string textKey,
            Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null,
            bool isReduceOnActive = false,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new AbilityCountBehavior(
                    text: Helper.Translation.GetString(textKey),
                    img: img,
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityCleanUp,
                    forceAbilityOff: forceAbilityOff,
                    isReduceOnActive: isReduceOnActive),
                new RoleButtonActivator(),
                hotKey
            );
        }

        public static ExtremeAbilityButton CreatePassiveAbility(
            string textKey,
            Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new PassiveAbilityBehavior(
                    text: Helper.Translation.GetString(textKey),
                    img: img,
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityCleanUp),
                new RoleButtonActivator(),
                hotKey
            );
        }

        public static ExtremeAbilityButton CreateReclickAbility(
            string textKey,
            Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new ReclickAbilityBehavior(
                    text: Helper.Translation.GetString(textKey),
                    img: img,
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityCleanUp),
                new RoleButtonActivator(),
                hotKey
            );
        }

        public static ExtremeAbilityButton CreateReusableAbility(
            string textKey,
            Sprite img,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityCleanUp = null,
            Action forceAbilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new ReusableAbilityBehavior(
                    text: Helper.Translation.GetString(textKey),
                    img: img,
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityCleanUp,
                    forceAbilityOff: forceAbilityOff),
                new RoleButtonActivator(),
                hotKey
            );
        }
    }
}
