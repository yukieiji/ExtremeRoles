using System;
using TMPro;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ToggleButtonBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
	private TextMeshProUGUI text;
	private ToggleButtonBodyBehaviour button;

	public void Awake()
	{
		var text = base.transform.Find("Text");
		if (text != null &&
			text.TryGetComponent<TextMeshProUGUI>(out var textBody))
		{
			this.text = textBody;
			this.text.text = "設定済みのみを表示";
		}
		var body = base.transform.Find("ButtonBody");
		if (body != null &&
			body.TryGetComponent<ToggleButtonBodyBehaviour>(out var buttonBody))
		{
			this.button = buttonBody;
		}
	}
}