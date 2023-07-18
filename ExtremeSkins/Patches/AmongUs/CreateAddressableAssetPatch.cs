using ExtremeSkins.Module;
using ExtremeSkins.SkinManager;

using Innersloth.Assets;

using HarmonyLib;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch]
public static class CreateAddressableAssetPatch
{
#if WITHHAT
	[HarmonyPrefix]
	[HarmonyPatch(typeof(HatData), nameof(HatData.CreateAddressableAsset))]
	public static bool HatDataPrefix(HatData __instance, ref AddressableAsset<HatViewData> __result)
	{
		if (ExtremeHatManager.HatData.TryGetValue(__instance.ProductId, out var value))
		{
			var asset = new HatAddressableAsset();
			asset.Init(value);
			__result = asset.Cast<AddressableAsset<HatViewData>>();
			return false;
		}
		return true;
	}
#endif

#if WITHNAMEPLATE
	[HarmonyPrefix]
	[HarmonyPatch(typeof(NamePlateData), nameof(NamePlateData.CreateAddressableAsset))]
	public static bool NamePlatePrefix(NamePlateData __instance, ref AddressableAsset<NamePlateViewData> __result)
	{
		if (ExtremeNamePlateManager.NamePlateData.TryGetValue(__instance.ProductId, out var value))
		{
			var asset = new NamePlateAddressableAsset();
			asset.Init(value);
			__result = asset.Cast<AddressableAsset<NamePlateViewData>>();
			return false;
		}
		return true;
	}
#endif

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisorData), nameof(VisorData.CreateAddressableAsset))]
	public static bool VisorPrefix()
	{
		ExtremeSkinsPlugin.Logger.LogInfo("aaaaaaaaa");
		return true;
	}
}