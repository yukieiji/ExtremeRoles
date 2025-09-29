using System;
using TMPro;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ToggleButtonBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
	private TextMeshProUGUI? text;
	private ToggleButtonBodyBehaviour? button;

	public void Awake()
	{
		var textBody = base.transform.Find("Text");
		if (textBody != null &&
			textBody.TryGetComponent<TextMeshProUGUI>(out var textBehav))
		{
			this.text = textBehav;
			this.text.text = "設定済みのみを表示";
		}
		var buttonBody = base.transform.Find("ButtonBody");
		if (buttonBody != null &&
			buttonBody.TryGetComponent<ToggleButtonBodyBehaviour>(out var buttonBehav))
		{
			this.button = buttonBehav;
			this.button.Awake();
		}
	}

	public void Initialize(string text, ToggleButtonBodyBehaviour.ColorProperty property, bool active, Action<bool> act)
	{
		if (this.text != null)
		{
			this.text.text = text;
		}
		if (this.button != null)
		{
			this.button.Initialize(property, active, act);
		}
	}
}