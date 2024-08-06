using System;

using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Resources;
using ExtremeRoles.Translation;

#nullable enable

namespace ExtremeRoles.Compat.SafeBoot;

public static class SafeBootScheduler
{

	internal static void Boot(in Harmony harmony)
	{
		harmony.UnpatchSelf();


		Il2CppRegisterAttribute.RegistrationForTarget(
			typeof(SimpleButton),
			Type.EmptyTypes);

		StatusTextShower.Instance.Add(
			() => Tr.GetString("SafeBootMessage"));

		UnityObjectLoader.LoadCommonAsset();

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

		Il2CppSystem.Collections.Generic.Dictionary<string, string>? allStrings = null;
		harmony.Patch(
			AccessTools.Method(
				typeof(LanguageUnit),
				nameof(LanguageUnit.ParseTSV)),
			postfix: new HarmonyMethod(
				SymbolExtensions.GetMethodInfo(
					() => SafeBootTranslationPatch.Postfix(ref allStrings)))
			);
		
		TranslatorManager.Register<Translator>();
	}
}
