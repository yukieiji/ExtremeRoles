using TMPro;

using UnityEngine;

namespace ExtremeRoles.Module.Ability.Behavior.Interface;

public interface ICountBehavior
{
	public const string DefaultButtonCountText = "buttonCountText";
	public int AbilityCount { get; }

	public void SetAbilityCount(int newAbilityNum);

	public void SetButtonTextFormat(string newTextFormat);

	public static TextMeshPro CreateCountText(ActionButton targetButton)
	{
		var coolTimerText = targetButton.cooldownTimerText;
		var text = Object.Instantiate(
			coolTimerText, coolTimerText.transform.parent);
		text.enableWordWrapping = false;
		text.transform.localScale = Vector3.one * 0.5f;
		text.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);

		return text;
	}
}
