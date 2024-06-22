using HarmonyLib;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;


#nullable enable

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class GameSettingMenuStartPatch
{
	private const float yScale = 0.85f;

	public static void Postfix(GameSettingMenu __instance)
	{
		using var dec = new ExtremeGameSettingMenu.Initializer(__instance);
		var menu = __instance.gameObject.TryAddComponent<ExtremeGameSettingMenu>();
		menu.Initialize(dec);
	}
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
public static class GameSettingMenuChangeTabPatch
{
	public static void Prefix(
		GameSettingMenu __instance,
		[HarmonyArgument(0)] int tabNum,
		[HarmonyArgument(1)] bool previewOnly)
	{
		if (__instance.TryGetComponent(out ExtremeGameSettingMenu menu))
		{
			menu.SwitchTabPrefix(previewOnly);
		}
	}
}
