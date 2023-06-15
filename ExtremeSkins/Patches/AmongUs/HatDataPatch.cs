using ExtremeSkins.Module;
using ExtremeSkins.SkinManager;

using Innersloth.Assets;

using HarmonyLib;
using System.Runtime.CompilerServices;
using ExtremeRoles.Performance;

namespace ExtremeSkins.Patches.AmongUs;

[HarmonyPatch(typeof(HatData), nameof(HatData.CreateAddressableAsset))]
public static class HatDataCreateAddressableAssetPatch
{
	public static bool Prefix(HatData __instance, ref AddressableAsset<HatViewData> __result)
	{
		if (ExtremeHatManager.HatData.TryGetValue(__instance.ProductId, out var value))
		{
			// 10番のハットを装備させる
			// var hat = FastDestroyableSingleton<HatManager>.Instance.allHats[10];
			// __result = new AddressableAsset<HatViewData>(hat.ViewDataRef);

			var asset = new HatAddressableAset();
			asset.Init(value);
			__result = asset.Cast<AddressableAsset<HatViewData>>();
			return false;
		}
		return true;
	}
}