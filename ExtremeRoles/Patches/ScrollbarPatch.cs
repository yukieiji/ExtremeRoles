using ExtremeRoles.Extension.Vector;
using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches;

// バニラに存在するスクロールバーをドラッグして移動する際のスタッター不具合の軽減策
[HarmonyPatch(typeof(Scrollbar), nameof(Scrollbar.ReceiveClickDrag))]
public static class ScrollbarReceiveClickDrag
{
	public static bool Prefix(Scrollbar __instance, [HarmonyArgument(0)] Vector2 dragDelta)
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
		=> dragDelta.IsNotCloseTo(Vector2.zero, 0.00125f);
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
}
