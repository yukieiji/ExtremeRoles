using System;

using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Resources;

using Trans = ExtremeRoles.Helper.Translation;

#nullable enable

namespace ExtremeRoles.Compat.SafeBoot;

public static class SafeBootScheduler
{

	internal static void Boot(in Harmony harmony)
	{
		harmony.UnpatchSelf();

		try
		{
			Trans.Load();
		}
		catch (Exception ex)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Can't load transdata\nMessage:{ex.Message}");
		}

		Il2CppRegisterAttribute.RegistrationForTarget(
			typeof(SimpleButton),
			Type.EmptyTypes);

		StatusTextShower.Instance.Add(
			() => Trans.GetString("SafeBootMessage"));

		Loader.LoadCommonAsset();

		MainMenuManager? menuManager = null;
		harmony.Patch(
			AccessTools.Method(
				typeof(MainMenuManager),
				nameof(MainMenuManager.Start)),
			new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeBootMainMenuManagerPatch.Prefix(menuManager))),
			new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeBootMainMenuManagerPatch.Postfix(menuManager)))
			);

		VersionShower? shower = null;
		harmony.Patch(
			AccessTools.Method(
				typeof(VersionShower),
				nameof(VersionShower.Start)),
			postfix: new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeBootVersionShowerPatch.Postfix(shower)))
			);
	}
}
