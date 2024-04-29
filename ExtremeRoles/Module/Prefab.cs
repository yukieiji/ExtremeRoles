using UnityEngine;
using TMPro;

namespace ExtremeRoles.Module;

public static class Prefab
{
	public static TextMeshPro Text;
	public static GenericPopup Prop;
	public static PoolablePlayer PlayerPrefab;

	public static GenericPopup CreateConfirmMenu(
		in System.Action okAction,
		StringNames okStr = StringNames.Continue,
		StringNames canselStr = StringNames.Cancel)
	{
		var result = Object.Instantiate(Prop);
		result.transform.localScale = new Vector3(2.0f, 1.25f, 1.0f);

		var okButton = result.transform.FindChild("ExitGame");
		Object.Destroy(okButton.GetComponentInChildren<TextTranslatorTMP>());
		var exitButton = Object.Instantiate(okButton, result.transform);

		okButton.transform.localScale = new Vector3(0.5f, 0.9f, 1.0f);
		exitButton.transform.localScale = new Vector3(0.5f, 0.9f, 1.0f);

		okButton.GetComponentInChildren<TextMeshPro>().text =
			TranslationController.Instance.GetString(okStr);
		exitButton.GetComponentInChildren<TextMeshPro>().text =
			TranslationController.Instance.GetString(canselStr);

		okButton.transform.SetLocalX(0.75f);
		exitButton.transform.SetLocalX(-0.75f);

		okButton.GetComponent<PassiveButton>().OnClick.AddListener(okAction);

		result.TextAreaTMP.transform.SetLocalX(-0.7f);

		var textScale = result.TextAreaTMP.transform.localScale;
		result.TextAreaTMP.transform.localScale = new Vector3(0.9f, 1.5f, textScale.z);
		result.TextAreaTMP.alignment = TextAlignmentOptions.TopLeft;

		return result;
	}
}
