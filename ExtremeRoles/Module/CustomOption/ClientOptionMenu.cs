using AmongUs.Data;
using System;
using System.Collections.Generic;
using ExtremeRoles.Patches.Meeting;


using UnityEngine;
using TMPro;

using ExtremeRoles.Extension.UnityEvents;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Module.CustomOption;

public sealed class ClientOptionMenu
{
	private sealed record SelectionBehaviour(
		string TitleKey, Func<bool> OnClick, bool DefaultValue);
	private readonly record struct OptionButton(
		ToggleButtonBehaviour Button,
		SelectionBehaviour Behaviour);

	private readonly GameObject popUp;
	private readonly TextMeshPro moreOptionText;
	private readonly IReadOnlyList<ToggleButtonBehaviour> buttons;

	private static SelectionBehaviour[] modOption => [
		new ("ghostsSeeTasksButton",
			() =>
			{
				bool newValue = !clientOpt.GhostsSeeTask.Value;
				clientOpt.GhostsSeeTask.Value = newValue;
				return newValue;
			}, clientOpt.GhostsSeeTask.Value),
		new ("ghostsSeeVotesButton",
			() =>
			{
				bool newValue = !clientOpt.GhostsSeeVote.Value;
				clientOpt.GhostsSeeVote.Value = newValue;
				return newValue;
			}, clientOpt.GhostsSeeVote.Value),
		new ("ghostsSeeRolesButton",
			() =>
			{
				bool newValue = !clientOpt.GhostsSeeRole.Value;
				clientOpt.GhostsSeeRole.Value = newValue;
				return newValue;
			}, clientOpt.GhostsSeeRole.Value),
		new ("showRoleSummaryButton",
			() =>
			{
				bool newValue = !clientOpt.ShowRoleSummary.Value;
				clientOpt.ShowRoleSummary.Value = newValue;
				return newValue;
			}, clientOpt.ShowRoleSummary.Value),
		new ("hideNamePlateButton",
			() =>
			{
				bool newValue = !clientOpt.HideNamePlate.Value;
				clientOpt.HideNamePlate.Value = newValue;
				NamePlateHelper.NameplateChange = true;
				return newValue;
			}, clientOpt.HideNamePlate.Value)
	];

	private static ClientOption clientOpt => ClientOption.Instance;

	public ClientOptionMenu(in OptionsMenuBehaviour optionMenu)
	{
		popUp = createCustomMenu(optionMenu);

		var buttonPrefab = createButtonPrefab(optionMenu);
	}

	private static void initializeCustomMenu(
		in ToggleButtonBehaviour prefab,
		in GameObject popUp)
	{
		var modOptionArr = modOption;
		var optionButton = new List<OptionButton>(modOptionArr.Length);

		for (int i = 0; i < modOptionArr.Length; i++)
		{
			var opt = modOptionArr[i];
			var button = UnityObject.Instantiate(
				prefab, popUp.transform);
			button.transform.localPosition = new Vector3(
				i % 2 == 0 ? -1.17f : 1.17f,
				1.75f - i / 2 * 0.8f,
			-.5f);

			button.onState = opt.DefaultValue;
			button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

			button.Text.text = "";
			button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
			button.Text.font = UnityObject.Instantiate(
				Module.Prefab.Text.font);
			button.Text.GetComponent<RectTransform>().sizeDelta =
				new Vector2(2, 2);

			button.name = opt.TitleKey.Replace(" ", "") + "toggle";
			button.gameObject.SetActive(true);
			button.gameObject.transform.SetAsFirstSibling();

			var passiveButton = button.GetComponent<PassiveButton>();
			var colliderButton = button.GetComponent<BoxCollider2D>();

			colliderButton.size = new Vector2(2.2f, .7f);

			passiveButton.OnClick.RemoveAllPersistentAndListeners();
			passiveButton.OnMouseOut.RemoveAllPersistentAndListeners();
			passiveButton.OnMouseOver.RemoveAllPersistentAndListeners();

			passiveButton.OnClick.AddListener(() =>
			{
				button.onState = opt.OnClick.Invoke();
				button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
			});

			passiveButton.OnMouseOver.AddListener(
				() =>
				{
					button.Background.color = new Color32(34, 139, 34, byte.MaxValue);
				});
			passiveButton.OnMouseOut.AddListener(
				() =>
				{
					button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
				});

			foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				spr.size = new Vector2(2.2f, .7f);
			}
			optionButton.Add(button);
		}
	}

	private static GameObject createCustomMenu(
		in OptionsMenuBehaviour prefab)
	{
		GameObject popUp = UnityObject.Instantiate(prefab.gameObject);
		popUp.name = "modMenu";
		UnityObject.DontDestroyOnLoad(popUp);
		var transform = popUp.transform;
		var pos = transform.localPosition;
		pos.z = -810f;
		transform.localPosition = pos;

		UnityObject.Destroy(
			popUp.GetComponent<OptionsMenuBehaviour>());
		foreach (var gObj in getAllChilds(popUp))
		{
			if (gObj.name != "Background" &&
				gObj.name != "CloseButton")
			{
				UnityObject.Destroy(gObj);
			}
		}

		popUp.SetActive(false);

		return popUp;
	}

	private static ToggleButtonBehaviour createButtonPrefab(
		in OptionsMenuBehaviour instance)
	{
		ToggleButtonBehaviour buttonPrefab = UnityObject.Instantiate(
			instance.CensorChatButton);
		buttonPrefab.name = "censorChatPrefab";
		buttonPrefab.gameObject.SetActive(false);

		return buttonPrefab;
	}

	private static IEnumerable<GameObject> getAllChilds(
		GameObject go)
	{
		for (int i = 0; i < go.transform.childCount; ++i)
		{
			yield return go.transform.GetChild(i).gameObject;
		}
	}
}
