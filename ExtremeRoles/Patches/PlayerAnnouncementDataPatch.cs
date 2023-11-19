
using HarmonyLib;

using AmongUs.Data.Player;
using Assets.InnerNet;

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Module;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
public static class PlayerAnnouncementDataSetAnnouncementsPatch
{
	public static void Prefix([HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
	{
		aRange = ModAnnounce.AddModAnnounce(aRange);
	}
}
