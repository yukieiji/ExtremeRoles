using System;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
public static class StringOptionDecreasePatch
{
	public static bool Prefix(StringOption __instance)
	{

		string idStr = __instance.gameObject.name.Replace(
			OptionMenuTab.StringOptionName, string.Empty);

		if (!int.TryParse(idStr, out int id)) { return true; };

		OptionManager.Instance.ChangeOptionValue(id, false);

		return false;
	}
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
public static class StringOptionIncreasePatch
{
	public static bool Prefix(StringOption __instance)
	{
		string idStr = __instance.gameObject.name.Replace(
			OptionMenuTab.StringOptionName, string.Empty);

		if (!int.TryParse(idStr, out int id)) { return true; };

		OptionManager.Instance.ChangeOptionValue(id, true);

		return false;
	}
}
[HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
public static class StringOptionOnEnablePatch
{
    public static bool Prefix(StringOption __instance)
    {
        string idStr = __instance.gameObject.name.Replace(
            OptionMenuTab.StringOptionName, string.Empty);

        if (!int.TryParse(idStr, out int id)) { return true; };

        IOptionInfo option = OptionManager.Instance.GetIOption(id);

        __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
        __instance.TitleText.text = option.GetTranslatedName();
        __instance.Value = __instance.oldValue = option.CurSelection;
        __instance.ValueText.text = option.GetTranslatedValue();

        return false;
    }
}
