using System;

using UnityEngine;

using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.ButtonAutoActivator;

namespace ExtremeRoles.Module.Ability.Factory;

public static class RoleAbilityFactory
{
	public static ExtremeAbilityButton CreateBatteryAbility(
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
			new BatteryBehavior(
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
			new CountBehavior(
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

	public static ExtremeAbilityButton CreateActivatingCountAbility(
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
			new CountBehavior(
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
			new PassiveBehavior(
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
			new ReclickBehavior(
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

	public static ExtremeAbilityButton CreateReclickCountAbility(
		string textKey,
		Sprite img,
		Func<bool> canUse,
		Func<bool> ability,
		Func<bool> canActivating = null,
		Action abilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{
		return new ExtremeAbilityButton(
			new ReclickCountBehavior(
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
			new ReusableBehavior(
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

	public static ExtremeAbilityButton CreateActivatingReusableAbility(
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
			new ReusableBehavior(
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
