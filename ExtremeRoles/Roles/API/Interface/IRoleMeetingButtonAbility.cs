using System;

using UnityEngine;

namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleMeetingButtonAbility
{
    public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance);

    public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton);

    public Action CreateAbilityAction(PlayerVoteArea instance);

    public Sprite AbilityImage { get; }

	protected static void DefaultButtonMod(PlayerVoteArea instance, UiElement abilityButton, string name)
	{
		abilityButton.name = $"{name}_{instance.TargetPlayerId}";
		var controllerHighlight = abilityButton.transform.FindChild("ControllerHighlight");
		if (controllerHighlight != null)
		{
			controllerHighlight.localScale *= new Vector2(1.25f, 1.25f);
		}
	}
}
