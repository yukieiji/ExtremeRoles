using AmongUs.Data.Player;
using HarmonyLib;

namespace ExtremeRoles.Patches;


[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
public static class PlayerBanDataPatch
{
	public static void Postfix(ref bool __result)
	{
		__result = false;
	}
}
