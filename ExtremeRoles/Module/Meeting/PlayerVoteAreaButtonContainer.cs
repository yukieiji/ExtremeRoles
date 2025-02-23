
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Module.Meeting;

public sealed class PlayerVoteAreaButtonContainer(PlayerVoteArea pva)
{
	private readonly PlayerVoteArea pva = pva;
	public PlayerVoteAreaButtonGroup Group { get; } = new PlayerVoteAreaButtonGroup(pva);
	private readonly Dictionary<ExtremeRoleId, UiElement> cache = new Dictionary<ExtremeRoleId, UiElement>(2);
	private const float xySize = 0.625f;

	public bool IsRecreateButtn(
		ExtremeRoleId id,
		IRoleMeetingButtonAbility buttonRole,
		out UiElement button)
	{
		bool result = false;
		if (!this.cache.TryGetValue(id, out var cacheButton) ||
			cacheButton == null)
		{
			cacheButton = Object.Instantiate(
				this.pva.CancelButton,
				this.pva.ConfirmButton.transform.parent);
			var passiveButton = cacheButton.GetComponent<PassiveButton>();
			passiveButton.OnClick.RemoveAllPersistentAndListeners();
			passiveButton.OnClick.AddListener(this.pva.Cancel);
			passiveButton.OnClick.AddListener(
				() => { cacheButton.gameObject.SetActive(false); });
			passiveButton.OnClick.AddListener(
				buttonRole.CreateAbilityAction(this.pva));

			buttonRole.ButtonMod(this.pva, cacheButton);
			if (cacheButton.TryGetComponent<SpriteRenderer>(out var renderer))
			{
				renderer.sprite = buttonRole.AbilityImage;
				renderer.transform.localScale *= new Vector2(xySize, xySize);
			}

			this.cache[id] = cacheButton;
			result = true;
		}
		button = cacheButton;
		button.gameObject.SetActive(true);
		return result;
	}
}
