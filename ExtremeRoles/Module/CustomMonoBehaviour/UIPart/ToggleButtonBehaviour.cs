using System;

using TMPro;
using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ToggleButtonBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
	private TextMeshProUGUI? text;
	private ToggleButtonBodyBehaviour? button;


	[HideFromIl2Cpp]
	public void Initialize(string text, ToggleButtonBodyBehaviour.ColorProperty property, bool active, Action<bool> act)
	{
		setUpObject();

		if (this.text != null)
		{
			this.text.text = text;
		}
		if (this.button != null)
		{
			this.button.Initialize(property, active, act);
		}
	}

	private void setUpObject()
	{
		if (this.text == null)
		{
			var textBody = base.transform.Find("Text");
			if (textBody != null &&
				textBody.TryGetComponent<TextMeshProUGUI>(out var textBehav))
			{
				this.text = textBehav;
			}
		}

		if (this.button == null)
		{
			var buttonBody = base.transform.Find("ButtonBody");
			if (buttonBody != null &&
				buttonBody.TryGetComponent<ToggleButtonBodyBehaviour>(out var buttonBehav))
			{
				this.button = buttonBehav;
			}
		}
	}
}