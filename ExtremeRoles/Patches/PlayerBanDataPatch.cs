using AmongUs.Data.Player;
using HarmonyLib;

namespace ExtremeRoles.Patches;


[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned))]
public static class PlayerBanDataPatch
{
	public static void Postfix(ref bool __result)
	{
		__result = false;
	}
}
