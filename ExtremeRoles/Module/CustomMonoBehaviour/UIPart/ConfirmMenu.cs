using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart
{
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
                Translation.GetString("ok"));
            this.cancelButton.SetButtonText(
                Translation.GetString("cancel"));
        }

        public void ResetButtonAction()
        {
            this.okButton.ResetButtonAction();
            this.cancelButton.ResetButtonAction();
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

        public void SetOkButtonClickAction(UnityAction act)
        {
            this.okButton.SetButtonClickAction(act);
        }

        public void SetCancelButtonClickAction(UnityAction act)
        {
            this.cancelButton.SetButtonClickAction(act);
        }
    }
}
