using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

using TMPro;
using UnityEngine;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Compat.Operator;

internal abstract class OperatorBase
{
	protected const string ReactorURL = "https://api.github.com/repos/NuclearPowered/Reactor/releases/latest";
	protected const string ReactorDll = "Reactor.dll";

	protected string ModFolderPath;
	protected GenericPopup Popup;

	internal OperatorBase()
	{

		this.ModFolderPath = System.IO.Path.GetDirectoryName(
			Application.dataPath) + @"\BepInEx\plugins";

		this.Popup = Object.Instantiate(Module.Prefab.Prop);
		this.Popup.TextAreaTMP.fontSize *= 0.7f;
		this.Popup.TextAreaTMP.enableAutoSizing = false;
	}

	protected static void ShowConfirmMenu(string text, System.Action okAction)
	{
		var result = Object.Instantiate(Module.Prefab.Prop);
		result.transform.localScale = new Vector3(2.0f, 1.25f, 1.0f);

		var okButton = result.transform.FindChild("ExitGame");
		Object.Destroy(okButton.GetComponentInChildren<TextTranslatorTMP>());
		var exitButton = Object.Instantiate(okButton, result.transform);

		okButton.transform.localScale = new Vector3(0.5f, 0.9f, 1.0f);
		exitButton.transform.localScale = new Vector3(0.5f, 0.9f, 1.0f);

		okButton.GetComponentInChildren<TextMeshPro>().text =
			TranslationController.Instance.GetString(StringNames.Continue);
		exitButton.GetComponentInChildren<TextMeshPro>().text =
			TranslationController.Instance.GetString(StringNames.Cancel);

		var curPos = okButton.transform.localPosition;
		okButton.transform.localPosition = new Vector3(0.75f, curPos.y, curPos.z);
		exitButton.transform.localPosition = new Vector3(-0.75f, curPos.y, curPos.z);

		okButton.GetComponent<PassiveButton>().OnClick.AddListener(okAction);

		var pos = result.TextAreaTMP.transform.localPosition;
		result.TextAreaTMP.transform.localPosition = new Vector3(-0.7f, pos.y, pos.z);
		var textScale = result.TextAreaTMP.transform.localScale;
		result.TextAreaTMP.transform.localScale = new Vector3(0.9f, 1.5f, textScale.z);
		result.TextAreaTMP.alignment = TextAlignmentOptions.TopLeft;

		result.Show(text);
	}

	protected static async Task<JObject?> GetRestApiDataAsync(HttpClient client, string url)
	{
		var req = await client.GetAsync(new System.Uri(url),
						HttpCompletionOption.ResponseContentRead);
		if (req.StatusCode != HttpStatusCode.OK || req.Content == null)
		{
			Logging.Error($"Server returned no data: {req.StatusCode}");
			return null;
		}
		string dataString = await req.Content.ReadAsStringAsync();
		JObject data = JObject.Parse(dataString);
		return data;
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
