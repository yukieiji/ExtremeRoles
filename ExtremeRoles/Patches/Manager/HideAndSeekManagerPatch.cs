using HarmonyLib;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(HideAndSeekManager), nameof(HideAndSeekManager.GetBodyType))]
public class HideAndSeekManagerGetBodyTypePatch
{
    public static bool Prefix(
        [HarmonyArgument(0)] PlayerControl player,
        out PlayerBodyTypes __result)
    {
        bool isKiller =
            player &&
            player.Data != null &&
            player.Data.Role &&
            player.Data.Role.IsImpostor;

        if (Constants.ShouldHorseAround())
        {
            __result = isKiller ?
                PlayerBodyTypes.Normal : PlayerBodyTypes.Horse;
        }
        else
        {
            __result = isKiller ?
                PlayerBodyTypes.Seeker : PlayerBodyTypes.Normal;
        }

        return false;
    }
}
