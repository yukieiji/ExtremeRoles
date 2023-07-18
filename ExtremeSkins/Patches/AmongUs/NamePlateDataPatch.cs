using ExtremeSkins.Module;
using ExtremeSkins.SkinManager;

using Innersloth.Assets;

using HarmonyLib;

namespace ExtremeSkins.Patches.AmongUs;

#if WITHNAMEPLATE

[HarmonyPatch(typeof(NamePlateData), nameof(NamePlateData.CreateAddressableAsset))]
public static class NamePlateDataCreateAddressableAssetPatch
{
	public static bool Prefix(NamePlateData __instance, ref AddressableAsset<NamePlateViewData> __result)
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
}
#endif