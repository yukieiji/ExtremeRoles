using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnhollowerBaseLib.Attributes;

namespace ExtremeSkins.Module
{
    [Il2CppRegister]
    public sealed class CreatorTab : MonoBehaviour
    {
        private ButtonWrapper buttonPrefab;
        private VerticalLayoutGroup layout;

        private List<ButtonWrapper> selectButton = new List<ButtonWrapper>();

        
        public void Awake()
        {
            Transform trans = base.transform;

            this.buttonPrefab = trans.Find(
                "Button").gameObject.GetComponent<ButtonWrapper>();
            this.layout = trans.Find(
                "ScrollView/Viewport/Content").gameObject.GetComponent<VerticalLayoutGroup>();
            this.buttonPrefab.Awake();

            this.selectButton.Clear();
        }

        [HideFromIl2Cpp]
        public void SetUpButtons(Scroller scroller, List<TMP_Text> creatorText)
        {
            foreach (TMP_Text text in creatorText)
            {
                ButtonWrapper newButton = Instantiate(
                    this.buttonPrefab,
                    this.layout.transform);
                CreatorButton creator = newButton.gameObject.AddComponent<CreatorButton>();
                creator.Initialize(scroller, text);
                newButton.gameObject.SetActive(true);
                newButton.SetButtonText(text.text);
                newButton.ResetButtonAction();

                newButton.SetButtonClickAction(
                    (UnityAction)creator.GetClickAction());
                this.selectButton.Add(newButton);
            }
        }
    }
}
