using System;

using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Compat.SafeBoot;

public static class SafeBootScheduler
{

	internal static void Boot(in Harmony harmony)
	{
		harmony.UnpatchSelf();

		try
		{
			Helper.Translation.Load();
		}
		catch (Exception ex)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Can't load transdata\nMessage:{ex.Message}");
		}

		Il2CppRegisterAttribute.RegistrationForTarget(
			typeof(SimpleButton),
			Type.EmptyTypes);

		StatusTextShower.Instance.Add(
			() => "何らかの不具合が発生したため\nExRを最低限の機能で起動しています");

		Loader.LoadCommonAsset();

		MainMenuManager? menuManager = null;
		harmony.Patch(
			AccessTools.Method(
				typeof(MainMenuManager),
				nameof(MainMenuManager.Start)),
			new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeModeMainMenuManagerPatch.Prefix(menuManager))),
			new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeModeMainMenuManagerPatch.Postfix(menuManager)))
			);

		VersionShower? shower = null;
		harmony.Patch(
			AccessTools.Method(
				typeof(VersionShower),
				nameof(VersionShower.Start)),
			postfix: new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeModeVersionShowerPatch.Postfix(shower)))
			);
	}
}
