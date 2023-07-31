using System;

using HarmonyLib;
using Innersloth.Assets;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Performance;
using ExtremeSkins.Module;
using ExtremeSkins.SkinManager;

using Enumerator = System.Collections.IEnumerator;
using Il2CppEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeSkins.Patches.AmongUs.Cosmic;

#if WITHVISOR

[HarmonyPatch]
public static class VisorPatch
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetVisor), new Type[] { typeof(VisorData), typeof(int) })]
	public static bool VisorLayerSetVisorPrefix(
		VisorLayer __instance,
		[HarmonyArgument(0)] VisorData data,
		[HarmonyArgument(1)] int colorId)
	{
		if (ExtremeVisorManager.VisorData.TryGetValue(data.ProductId, out var visor))
		{
			__instance.currentVisor = data;
			__instance.UnloadAsset();
			__instance.viewAsset = VisorAddressableAsset.CreateAsset(visor);
			__instance.LoadAssetAsync(__instance.viewAsset, (Il2CppSystem.Action)(() =>
			{
				if (__instance.viewAsset.GetAsset() == null)
				{
					return;
				}
				if (__instance.IsDestroyedOrNull() || __instance.gameObject.IsDestroyedOrNull())
				{
					return;
				}
				__instance.SetVisor(__instance.currentVisor, __instance.viewAsset.GetAsset(), colorId);
			}), null);
			return false;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisorData), nameof(VisorData.CoLoadIcon))]
	public static bool CoLoadIconPrefix(
		VisorData __instance,
		[HarmonyArgument(0)] Il2CppSystem.Action<Sprite, AddressableAsset> onLoaded,
		ref Il2CppEnumerator __result)
	{
		if (ExtremeVisorManager.VisorData.TryGetValue(__instance.ProductId, out var value))
		{
			__result = patchedCoLoadIcon(onLoaded, value).WrapToIl2Cpp();
			return false;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CosmeticsCache), nameof(CosmeticsCache.CoAddVisor))]
	public static bool CoAddVisorPrefix(
		CosmeticsCache __instance,
		[HarmonyArgument(0)] string visorId,
		ref Il2CppEnumerator __result)
	{
		__result = patchedCoAddVisor(__instance, visorId).WrapToIl2Cpp();
		return false;
	}

	private static Enumerator patchedCoAddVisor(CosmeticsCache instance, string visorId)
	{
		if (instance.visors.ContainsKey(visorId)) { yield break; }

		AddressableAsset<VisorViewData>? asset =
			ExtremeVisorManager.VisorData.TryGetValue(visorId, out var visor) ?
			VisorAddressableAsset.CreateAsset(visor) :
			FastDestroyableSingleton<HatManager>.Instance.GetVisorById(visorId).CreateAddressableAsset();
		instance.allCachedAssets.Add(asset);
		yield return asset.CoLoadAsync(null);
		instance.visors[visorId] = asset;

		asset = null;

		yield break;
	}

#pragma warning disable CS8600, CS8604
	private static Enumerator patchedCoLoadIcon(Il2CppSystem.Action<Sprite, AddressableAsset> onLoaded, CustomVisor visor)
	{
		AddressableAsset<VisorViewData> asset = VisorAddressableAsset.CreateAsset(visor);
		yield return asset.CoLoadAsync(null);
		VisorViewData asset2 = asset.GetAsset();
		Sprite sprite = ((asset2 != null) ? asset2.IdleFrame : null);
		onLoaded.Invoke(sprite, asset);
		yield break;
	}
#pragma warning restore CS8600, CS8604

}
#endif