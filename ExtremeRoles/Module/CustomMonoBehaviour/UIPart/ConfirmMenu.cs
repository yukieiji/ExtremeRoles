using System;

using UnityEngine;
using UnityEngine.Events;

using TMPro;
using Il2CppInterop.Runtime.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ConfirmMenu : MonoBehaviour
{
	private ButtonWrapper okButton;

	private ButtonWrapper cancelButton;

	private TextMeshProUGUI text;

	public ConfirmMenu(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		Transform trans = base.gameObject.transform;
		this.okButton = trans.Find(
			"OkButton").gameObject.GetComponent<ButtonWrapper>();
		this.cancelButton = trans.Find(
			"CancelButton").gameObject.GetComponent<ButtonWrapper>();
		this.text = trans.Find(
			"BodyText").gameObject.GetComponent<TextMeshProUGUI>();

		this.okButton.Awake();
		this.cancelButton.Awake();

		this.okButton.SetButtonText(
			TranslationController.Instance.GetString(StringNames.OK));
		this.cancelButton.SetButtonText(
			TranslationController.Instance.GetString(StringNames.Cancel));
	}

	public void ResetButtonAction()
	{
		ResetOkButtonAction();
		this.cancelButton.ResetButtonAction();
	}

	public void ResetOkButtonAction()
	{
		this.okButton.ResetButtonAction();
	}

	public void Update()
	{
		var meetingHud = MeetingHud.Instance;

		if (!meetingHud ||
			meetingHud.state == MeetingHud.VoteStates.Results ||
			Input.GetKeyDown(KeyCode.Escape))
		{
			this.gameObject.SetActive(false);
		}
	}

	public void SetMenuText(string text)
	{
		this.text.text = text;
	}

	[HideFromIl2Cpp]
	public void SetOkButtonClickAction(Delegate act)
	{
		this.okButton.SetButtonClickAction((UnityAction)act);
	}

	[HideFromIl2Cpp]
	public void SetOkButtonClickAction(Action act)
	{
		this.okButton.SetButtonClickAction((UnityAction)act);
	}

	public void SetOkButtonClickAction(UnityAction act)
	{
		this.okButton.SetButtonClickAction(act);
	}

	[HideFromIl2Cpp]
	public void SetCancelButtonClickAction(Delegate act)
	{
		this.cancelButton.SetButtonClickAction((UnityAction)act);
	}

	public void SetCancelButtonClickAction(UnityAction act)
	{
		this.cancelButton.SetButtonClickAction(act);
	}
}
