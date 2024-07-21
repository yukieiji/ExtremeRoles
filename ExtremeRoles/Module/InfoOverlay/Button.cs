using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.InfoOverlay;

public sealed class HelpButton
{
	public bool IsInitialized => this.body != null;

	private GameObject? body = null;

	public void CreateInfoButton(System.Action openAct)
	{
		this.body = Object.Instantiate(
			GameObject.Find("MenuButton"),
			GameObject.Find("TopRight/MenuButton").transform);
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
		this.body.transform.localPosition = new Vector3(-1.075f, 0.0f, 0.0f);

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
}
