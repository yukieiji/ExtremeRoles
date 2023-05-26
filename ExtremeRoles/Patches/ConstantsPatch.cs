using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;

using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
public static class ConstantsShouldHorseAroundPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result =
            ExtremeGameModeManager.Instance is not null &&
            ExtremeGameModeManager.Instance.ShipOption.CanUseHorseMode &&
            OptionManager.Instance.GetValue<bool>((int)GlobalOption.EnableHorseMode);
        return false;
    }
}
