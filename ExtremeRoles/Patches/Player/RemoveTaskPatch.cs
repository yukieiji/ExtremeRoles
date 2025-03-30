using HarmonyLib;

using ExtremeRoles.Patches.Manager;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveTask))]
public static class PlayerControlRemoveTaskPatch
{
	public static void Prefix()
	{
		HudManagerUpdatePatch.SetBlockUpdate(true);
		HudManager.Instance.taskDirtyTimer = 0.0f;
	}
	public static void Postfix()
	{
		HudManagerUpdatePatch.SetBlockUpdate(false);
	}
}
