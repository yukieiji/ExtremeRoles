using ExtremeSkins.Module;
using ExtremeSkins.SkinManager;

using Innersloth.Assets;

using HarmonyLib;


namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(HatData), nameof(HatData.CreateAddressableAsset))]
public static class HatDataCreateAddressableAssetPatch
{
	public static bool Prefix(HatData __instance, ref AddressableAsset<HatViewData> __result)
	{
		if (ExtremeHatManager.HatData.TryGetValue(__instance.ProductId, out var value))
		{
			__result = new AddressableAssetWrapper<HatData, HatViewData>(value);
		}
		return true;
	}
}