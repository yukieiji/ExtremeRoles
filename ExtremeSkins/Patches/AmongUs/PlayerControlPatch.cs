using Hazel;
using HarmonyLib;

using ExtremeSkins.Loader;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
public static class PlayerControlCheckColorPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
    {
        // Fix incorrect color assignment
        int color = bodyColor;

        if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length)
        {
            int num = 0;
            while (num++ < 50 && (color >= CustomColorLoader.AllColorNum || isTaken(__instance, color)))
            {
                color = (color + 1) % CustomColorLoader.AllColorNum;
            }
        }
        __instance.RpcSetColor((byte)color);
        return false;
    }

    private static bool isTaken(PlayerControl player, int color)
    {
        foreach (NetworkedPlayerInfo info in GameData.Instance.AllPlayers)
        {
            if (!info.Disconnected &&
                info.PlayerId != player.PlayerId &&
                info.DefaultOutfit.ColorId == color)
            {
                return true;
            }
        }
        return false;
    }

}
