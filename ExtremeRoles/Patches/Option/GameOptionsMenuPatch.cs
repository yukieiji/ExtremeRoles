using HarmonyLib;

using System.Linq;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Il2Cpp;


namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
public static class GameOptionsMenuCreateSettingsPatch
{
	public static void Postfix(GameOptionsMenu __instance)
	{
		var child = __instance.Children.ToArray();

		if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
			ServerManager.Instance.IsCustomServer())
		{
			changeValueRange(child, StringNames.GameNumImpostors, 0f, GameSystem.MaxImposterNum);
		}
		
		changeValueRange(child, StringNames.GameCommonTasks, 0f, 4f);
		changeValueRange(child, StringNames.GameShortTasks, 0f, 23f);
		changeValueRange(child, StringNames.GameLongTasks, 0f, 15f);
	}

	private static void changeValueRange(
	   Il2CppArrayBase<OptionBehaviour> child,
	   StringNames name, float minValue, float maxValue)
	{
		if (!(
				child.tryGetOption(name, out OptionBehaviour opt) &&
				opt.IsTryCast<NumberOption>(out var numOpt)
			))
		{
			return;
		}

		numOpt.ValidRange = new FloatRange(minValue, maxValue);
	}

	private static OptionBehaviour tryGetOption(
		this Il2CppArrayBase<OptionBehaviour> child,
		StringNames name, out OptionBehaviour optionBehaviour)
	{
		optionBehaviour = child.FirstOrDefault(x => x.Title == name);

		return optionBehaviour;
	}

}
