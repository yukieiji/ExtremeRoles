using System;

using UnityEngine;

using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.AutoActivator;

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
				text: Tr.GetString(textKey),
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
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{
		return new ExtremeAbilityButton(
			new CountBehavior(
				text: Tr.GetString(textKey),
				img: img,
				canUse: canUse,
				ability: ability,
				abilityOff: abilityOff,
				forceAbilityOff: forceAbilityOff),
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
			new ActivatingCountBehavior(
				text: Tr.GetString(textKey),
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
					Text = Tr.GetString(activateTextKey),
					Img = activateImg,
				},
				new ButtonGraphic
				{
					Text = Tr.GetString(deactivateTextKey),
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
				text: Tr.GetString(textKey),
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
				text: Tr.GetString(textKey),
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
		Action abilityOff = null,
		Action forceAbilityOff = null,
		KeyCode hotKey = KeyCode.F)
	{
		return new ExtremeAbilityButton(
			new ReusableBehavior(
				text: Tr.GetString(textKey),
				img: img,
				canUse: canUse,
				ability: ability,
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
			new ReusableActivatingBehavior(
				text: Tr.GetString(textKey),
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
