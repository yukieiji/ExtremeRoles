using HarmonyLib;

namespace ExtremeRoles.Patches.MiniGame;

// From : https://github.com/SubmergedAmongUs/Submerged/blob/main/Submerged/Minigames/Patches/FixMissingLoggerPatches.cs
[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
public static class MinigameBeginPatch
{
	private static readonly Logger logger = new Logger(Logger.Category.Gameplay, "Minigame");

	public static void Prefix(Minigame __instance)
	{
		__instance.logger ??= logger;
	}
}
