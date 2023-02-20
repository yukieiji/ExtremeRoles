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
            Action abilityOff = null,
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
                    abilityOff: abilityOff,
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
            Action abilityOff = null,
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
                    abilityOff: abilityOff,
                    forceAbilityOff: forceAbilityOff,
                    isReduceOnActive: isReduceOnActive),
                new RoleButtonActivator(),
                hotKey
            );
        }

        public static ExtremeAbilityButton CreatePassiveAbility(
            string activateTextKey,
            Sprite activateImg,
            string deactivateTextKey,
            Sprite deactivateImg,
            Func<bool> canUse,
            Func<bool> ability,
            Func<bool> canActivating = null,
            Action abilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new PassiveAbilityBehavior(
                    new ButtonGraphic
                    {
                        Text = Helper.Translation.GetString(activateTextKey),
                        Img = activateImg,
                    },
                    new ButtonGraphic
                    {
                        Text = Helper.Translation.GetString(deactivateTextKey),
                        Img = deactivateImg,
                    },
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityOff),
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
            Action abilityOff = null,
            KeyCode hotKey = KeyCode.F)
        {
            return new ExtremeAbilityButton(
                new ReclickAbilityBehavior(
                    text: Helper.Translation.GetString(textKey),
                    img: img,
                    canUse: canUse,
                    ability: ability,
                    canActivating: canActivating,
                    abilityOff: abilityOff),
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
            Action abilityOff = null,
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
                    abilityOff: abilityOff,
                    forceAbilityOff: forceAbilityOff),
                new RoleButtonActivator(),
                hotKey
            );
        }
    }
}
