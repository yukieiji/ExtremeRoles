using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Helper;
using HarmonyLib;

namespace ExtremeRoles.Test.Patches;


[HarmonyPatch(typeof(Player), nameof(Player.RpcUncheckSnap))]
public static class RpcSnapHookCheck
{
	public static List<Vector2>? Pos;

	public static void Postfix(
		[HarmonyArgument(0)] byte targetPlayerId,
		[HarmonyArgument(1)] Vector2 pos,
		[HarmonyArgument(2)] bool isTeleportXion)
	{
		if (Pos is null)
		{
			return;
		}
		Pos.Add(pos);
	}
}
