using HarmonyLib;
using Innersloth.Assets;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Performance;
using ExtremeSkins.Module;

using Enumerator = System.Collections.IEnumerator;
using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeSkins.Patches.AmongUs.Cosmic;

#if WITHHAT

[HarmonyPatch]
public static class HatPatch
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(HatParent), nameof(HatParent.SetHat), [ typeof(int) ])]
	public static bool VisorLayerSetVisorPrefix(
		HatParent __instance,
		[HarmonyArgument(0)] int color)
	{
		if (__instance.Hat != null &&
			CosmicStorage<CustomHat>.TryGet(__instance.Hat.ProductId, out var hat) &&
			hat != null)
		{
			__instance.SetMaterialColor(color);
			__instance.UnloadAsset();
			__instance.viewAsset = HatAddressableAsset.CreateAsset(hat);
			__instance.viewAsset.LoadAsync((Il2CppSystem.Action)(() =>
			{
				__instance.PopulateFromViewData();
			}), null, null);
			return false;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisorData), nameof(HatData.CoLoadIcon))]
	public static bool CoLoadIconPrefix(
		VisorData __instance,
		[HarmonyArgument(0)] Il2CppSystem.Action<Sprite, AddressableAsset> onLoaded,
		ref Il2CppEnumerator __result)
	{
		if (CosmicStorage<CustomHat>.TryGet(__instance.ProductId, out var value) &&
			value != null)
		{
			__result = patchedCoLoadIcon(onLoaded, value).WrapToIl2Cpp();
			return false;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.CoAddHat))]
	public static bool CoAddVisorPrefix(
		CosmeticsCache __instance,
		[HarmonyArgument(0)] string id,
		ref Il2CppEnumerator __result)
	{
		__result = patchedCoAddHat(__instance, id).WrapToIl2Cpp();
		return false;
	}

	private static Enumerator patchedCoAddHat(CosmeticsCache instance, string id)
	{
		if (instance.hats.ContainsKey(id)) { yield break; }

		AddressableAsset<HatViewData>? asset;

		if (CosmicStorage<CustomHat>.TryGet(id, out var hat) && hat != null)
		{
			asset = HatAddressableAsset.CreateAsset(hat);
		}
		else
		{
			var hatData = FastDestroyableSingleton<HatManager>.Instance.GetHatById(id);
			if (hatData == null) { yield break; }
			asset = hatData.CreateAddressableAsset();
		}
		instance.allCachedAssets.Add(asset);
		yield return asset.CoLoadAsync(null);
		instance.hats[id] = asset;

		asset = null;

		yield break;
	}

#pragma warning disable CS8600, CS8604
	private static Enumerator patchedCoLoadIcon(Il2CppSystem.Action<Sprite, AddressableAsset> onLoaded, CustomHat hat)
	{
		AddressableAsset<HatViewData> asset = HatAddressableAsset.CreateAsset(hat);
		yield return asset.CoLoadAsync(null);
		HatViewData asset2 = asset.GetAsset();
		Sprite sprite = ((asset2 != null) ? asset2.MainImage : null);
		onLoaded.Invoke(sprite, asset);
		yield break;
	}
#pragma warning restore CS8600, CS8604

}
#endif