using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.InfoOverlay;

public sealed class HelpButton
{
	public bool IsInitialized => this.body != null;

	private GameObject? body = null;
	private static GameObject MenuButton => GameObject.Find("MenuButton");
	private static PassiveButton InfoButton => HudManager.Instance.MatchInfoButton;

	public void CreateInfoButton(System.Action openAct)
	{
		var menuButton = MenuButton;
		this.body = Object.Instantiate(
			menuButton, menuButton.transform);
		Object.DontDestroyOnLoad(this.body);

		this.body.name = "infoRoleButton";
		this.body.SetActive(true);
		this.body.layer = 5;

		var passiveButton = this.body.GetComponent<PassiveButton>();
		passiveButton.OnClick.RemoveAllPersistentAndListeners();
		passiveButton.OnClick.AddListener(openAct);

		if (passiveButton.TryGetComponent<AspectPosition>(out var aspect))
		{
			Object.Destroy(aspect);
		}
		this.body.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		passiveButton.inactiveSprites.GetComponent<SpriteRenderer>().sprite = UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(
				ObjectPath.CommonImagePathFormat,
				ObjectPath.HelpNoneActiveImage));


		var activeSprite = UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(
				ObjectPath.CommonImagePathFormat,
				ObjectPath.HelpActiveImage));

		passiveButton.selectedSprites.GetComponent<SpriteRenderer>().sprite = activeSprite;
		passiveButton.activeSprites.GetComponent<SpriteRenderer>().sprite = activeSprite;
	}

	public void SetLobbyParent()
	{
		if (this.body != null)
		{
			this.body.transform.SetParent(MenuButton.transform);
			this.body.transform.localPosition = new Vector3(-1.275f, 0.0f, 0.0f);
		}
	}

	public void SetGameParent()
	{
		if (this.body != null)
		{
			this.body.transform.SetParent(InfoButton.transform);
			this.body.transform.localPosition = new Vector3(-0.75f, 0.0f, 0.0f);
		}
	}
}
