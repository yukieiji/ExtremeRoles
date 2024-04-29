using UnityEngine;


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
