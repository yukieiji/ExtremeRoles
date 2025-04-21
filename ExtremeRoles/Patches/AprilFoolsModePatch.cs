using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Patches;

/*
[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldHorseAround))]
public static class AprilFoolsModeShouldHorseAroundPatch
{
    public static bool Prefix(ref bool __result)
    {
		if (AprilFoolsMode.ShouldLongAround())
		{
			return true;
		}

		__result =
			ExtremeGameModeManager.Instance is not null &&
			ExtremeGameModeManager.Instance.ShipOption.CanUseHorseMode &&
			OptionManager.Instance.GetValue<bool>((int)GlobalOption.EnableHorseMode);
        return false;
    }
}
*/

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
public static class AprilFoolsModeShouldShowAprilFoolsTogglePatch
{
	public static void Postfix(CreateGameOptions __instance)
	{
		var mng = ServerManager.Instance;
		__instance.AprilFoolsToggle.SetActive(
			mng != null && mng.IsCustomServer());
	}
}