using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using HarmonyLib;

namespace ExtremeRoles.Patches.MapModule;

#nullable enable

[HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
public static class MushroomStartSporeTriggerPatch
{
	public static bool Prefix(Mushroom __instance)
	{
		string name = __instance.name;

		if (!name.StartsWith(ModedMushroomSystem.MushroomName)) { return true; }


		PlayerControl? localPlayer = PlayerControl.LocalPlayer;

		if (!__instance.mushroomCollider.enabled ||
			localPlayer == null ||
			!__instance.mushroomCollider.IsTouching(localPlayer.Collider))
		{
			return false;
		}

		string idStr = name.Split('_')[^1];

		if (!ExtremeSystemTypeManager.Instance.ExistSystem(ModedMushroomSystem.Type) ||
			!int.TryParse(idStr, out int id))
		{
			return false;
		}

		__instance.mushroomCollider.enabled = false;
		__instance.StartCoroutine(__instance.WaitForSpores());
		ModedMushroomSystem.RpcSporeModMushroom(id);

		return false;
	}
}
