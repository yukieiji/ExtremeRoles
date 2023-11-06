using ExtremeRoles.Module.SystemType;

using HarmonyLib;

namespace ExtremeRoles.Patches.MapModule;


[HarmonyPatch(typeof(Mushroom), nameof(Mushroom.StartSporeTrigger))]
public static class MushroomStartSporeTriggerPatch
{
	public static bool Prefix(Mushroom __instance)
	{
		string name = __instance.name;

		Helper.Logging.Debug(name);

		if (!name.StartsWith(ModedMushroomSystem.MushroomName)) { return true; }

		string idStr = name.Split('_')[^1];
		Helper.Logging.Debug(idStr);
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
