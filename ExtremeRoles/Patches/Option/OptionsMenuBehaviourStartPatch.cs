using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Patches.Option;


[HarmonyPatch]
[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Open))]
public static class OptionsMenuBehaviourStartPatch
{
	private static ModOptionMenu menu;

    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (!__instance.CensorChatButton) { return; }

		if (menu == null || menu.IsReCreate)
		{
			menu = new ModOptionMenu(__instance);
			setLeaveGameButtonPostion();
		}
		menu.Hide();
		menu.UpdateTranslation();
	}

    private static void setLeaveGameButtonPostion()
    {
        var leaveGameButton = GameObject.Find("LeaveGameButton");
        if (leaveGameButton == null) { return; }
        leaveGameButton.transform.localPosition += (Vector3.right * 1.3f);
    }
}
