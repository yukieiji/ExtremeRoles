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
		this.body.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		SetInfoButtonToGameStartShipPositon();

		var passiveButton = this.body.GetComponent<PassiveButton>();
		passiveButton.OnClick.RemoveAllPersistentAndListeners();
		passiveButton.OnClick.AddListener(openAct);

		var render = this.body.GetComponent<SpriteRenderer>();
		render.sprite = Loader.GetUnityObjectFromResources<Sprite>(
			Path.CommonAsset,
			string.Format(Path.GeneralImagePathFormat, "Help"));
	}

	public void SetInfoButtonToGameStartShipPositon()
	{
		if (this.body == null) { return; }
		this.body.transform.localPosition = new Vector3(
			0.0f, -0.825f, 0.0f);
	}

	public void SetInfoButtonToInGamePositon()
	{
		if (this.body == null) { return; }
		this.body.SetActive(true);
		this.body.transform.localPosition = new Vector3(
			0.0f, -1.75f, 0.0f);
	}
}
