using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Attributes;

using TMPro;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.InfoOverlay;
using ExtremeRoles.Module.InfoOverlay.Model;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class InfoOverlayView(IntPtr ptr) : MonoBehaviour(ptr)
{
#pragma warning disable CS8618
	private TextMeshProUGUI title;
	private TextMeshProUGUI info;

	private TextMeshProUGUI mainText;
	private TextMeshProUGUI subText;

	private GameObject pageButtonParent;
	private Button leftButton;
	private Button rightButton;

	private ButtonWrapper button;
#pragma warning restore CS8618

	private readonly SortedDictionary<InfoOverlayModel.Type, ButtonWrapper> menu =
		new SortedDictionary<InfoOverlayModel.Type, ButtonWrapper>();

	public void Awake()
	{
		Transform trans = base.transform;

		this.title = trans.Find("Title").GetComponent<TextMeshProUGUI>();
		this.info = trans.Find("PageChangeButton/InfoText").GetComponent<TextMeshProUGUI>();
		this.pageButtonParent = this.info.transform.parent.gameObject;

		this.leftButton = trans.Find("PageChangeButton/Up").GetComponent<Button>();
		this.rightButton = trans.Find("PageChangeButton/Down").GetComponent<Button>();

		this.button = trans.Find("ButtonAnchor/Button").GetComponent<ButtonWrapper>();

		this.mainText = trans.Find("InfoMain/Viewport/Content").GetComponent<TextMeshProUGUI>();
		this.subText = trans.Find("InfoSub/Viewport/Content").GetComponent<TextMeshProUGUI>();
	}

	[HideFromIl2Cpp]
	public void UpdateFromModel(InfoOverlayModel model)
	{
		if (this.menu.Count != model.PanelModel.Count)
		{
			foreach (var (button, index) in this.menu.Values.Select((value, index) => (value, index)))
			{
				if (index == 0) { continue; }

				if (button != null)
				{
					DestroyImmediate(button.gameObject);
				}
			}

			this.menu.Clear();
			this.button.Awake();
			this.button.ResetButtonAction();

			foreach (var (panel, index) in model.PanelModel.Select((value, index) => (value, index)))
			{
				ButtonWrapper newButton;
				if (index == 0)
				{
					newButton = this.button;
				}
				else
				{
					newButton = Instantiate(this.button, this.button.transform.parent);
					newButton.transform.localPosition = new Vector3(0.0f, index * -55.0f);

				}
				newButton.Awake();
				newButton.SetButtonText(Tr.GetString(panel.Key.ToString()));
				newButton.SetButtonClickAction(
					() =>
					{
						Update.SwithTo(model, panel.Key);
					});
				this.menu.Add(panel.Key, newButton);
			}
			this.button.SetEnable(true);
			if (this.menu.TryGetValue(model.CurShow, out var selectedButton))
			{
				selectedButton.SetEnable(false);
			}
		}
		else
		{
			foreach (var (showType, button) in this.menu)
			{
				button.SetEnable(showType != model.CurShow);
			}
		}

		if (model.PanelModel.TryGetValue(model.CurShow, out var panelModel) &&
			panelModel is not null)
		{
			var (main, sub) = panelModel.GetInfoText();

			this.title.text = Tr.GetString(model.CurShow.ToString());
			this.mainText.text = main;
			this.subText.text = sub;

			this.pageButtonParent.SetActive(false);
			this.leftButton.onClick.RemoveAllListeners();
			this.rightButton.onClick.RemoveAllListeners();

			switch (panelModel)
			{
				case PanelPageModelBase pageModel:
					this.pageButtonParent.SetActive(true);
					this.info.text = $"({pageModel.CurPage + 1}/{pageModel.PageNum})   {Tr.GetString("changePageMore")}";
					this.rightButton.onClick.AddListener(
						() =>
						{
							Update.DecreasePage(model);
						});
					this.leftButton.onClick.AddListener(
						() =>
						{
							Update.IncreasePage(model);
						});
					break;
				default:
					break;
			}
		}
	}
}
