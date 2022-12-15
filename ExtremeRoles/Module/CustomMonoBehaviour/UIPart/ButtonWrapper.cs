using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart
{
    [Il2CppRegister]
    public sealed class ButtonWrapper : MonoBehaviour
    {
        private Button button;

        private TextMeshProUGUI text;

        public ButtonWrapper(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            this.button = base.GetComponent<Button>();
            this.text = base.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void ResetButtonAction()
        {
            this.button.onClick.RemoveAllListeners();
        }

        public void SetButtonText(string showText)
        {
            this.text.text = showText;
        }

        public void SetButtonClickAction(UnityAction act)
        {
            this.button.onClick.AddListener(act);
        }
    }
}