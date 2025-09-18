using AmongUs.GameOptions;
using HarmonyLib;
using System;

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.SetUpFromData))]
public static class NumberOptionSetUpFromDataPatch
{
    public static void Prefix(
		NumberOption __instance,
		[HarmonyArgument(0)] BaseGameSetting data)
    {
		if (!(
				GameOptionsManager.Instance != null &&
				GameOptionsManager.Instance.CurrentGameOptions != null &&
				data.Title is StringNames.GameNumImpostors &&
				GameOptionsManager.Instance.CurrentGameOptions.TryGetInt(Int32OptionNames.MaxPlayers, out int num)
			))
		{
			return;
		}
		
		var array = GameOptionsManager.Instance.CurrentGameOptions.GetIntArray(Int32ArrayOptionNames.MaxImpostors);
		if (array == null ||
			array.Length < num)
		{
			return;
		}
		GameOptionsManager.Instance.CurrentGameOptions.SetInt(Int32OptionNames.MaxPlayers, array.Length - 1);
	}
}
