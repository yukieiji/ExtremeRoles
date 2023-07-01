using TMPro;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Compat.Excuter;

internal abstract class ButtonExcuterBase
{
    protected string modFolderPath;
    protected GenericPopup Popup;

    internal ButtonExcuterBase()
    {

        this.modFolderPath = System.IO.Path.GetDirectoryName(
            Application.dataPath) + @"\BepInEx\plugins";

        this.Popup = Object.Instantiate(Module.Prefab.Prop);
        this.Popup.TextAreaTMP.fontSize *= 0.7f;
        this.Popup.TextAreaTMP.enableAutoSizing = false;
    }

				protected static void ShowConfirmMenu(string text, System.Action okAction)
				{
								var result = Object.Instantiate(Module.Prefab.Prop);
								result.transform.localScale = new Vector3(1.5f, 1.0f, 1.0f);

								var okButton = result.transform.FindChild("ExitGame");
								Object.Destroy(okButton.GetComponentInChildren<TextTranslatorTMP>());
								var exitButton = Object.Instantiate(okButton, result.transform);

								okButton.transform.localScale = new Vector3(0.75f, 1.0f, 1.0f);
								exitButton.transform.localScale = new Vector3(0.75f, 1.0f, 1.0f);

								okButton.GetComponentInChildren<TextMeshPro>().text =
												TranslationController.Instance.GetString(StringNames.Continue);
								exitButton.GetComponentInChildren<TextMeshPro>().text =
												TranslationController.Instance.GetString(StringNames.Cancel);

								var curPos = okButton.transform.localPosition;
								okButton.transform.localPosition = new Vector3(0.75f, curPos.y, curPos.z);
								exitButton.transform.localPosition = new Vector3(-0.75f, curPos.y, curPos.z);

								okButton.GetComponent<PassiveButton>().OnClick.AddListener(okAction);

								var textScale = result.TextAreaTMP.transform.localScale;
								result.TextAreaTMP.transform.localScale = new Vector3(0.75f, textScale.y, textScale.z);

								result.Show(text);
				}

    protected void ShowPopup(string message)
    {
        SetPopupText(message);
        Popup.gameObject.SetActive(true);
    }

    protected void SetPopupText(string message)
    {
        if (Popup == null)
        {
            return;
        }

        if (Popup.TextAreaTMP != null)
        {
            Popup.TextAreaTMP.text = message;
        }
    }

    public abstract void Excute();
}
