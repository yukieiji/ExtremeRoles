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