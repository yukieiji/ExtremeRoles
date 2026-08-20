using HarmonyLib;

namespace ExtremeRoles.Patches;


/*
// ローカルゲームで部屋が作れない不具合の修正
// AmongUsくん？お前が始めた物語なのになんでローカルゲームで部屋が作れないんだよおおおおおおおおおおおおお！！！！！！！！！！！！！！！
[HarmonyPatch(
    typeof(CurrentModRegistration),
    nameof(CurrentModRegistration.TryGetModRegistrationGuid))]
public static class CurrentModRegistrationTryGetModRegistrationGuidPatch
{
	public static bool Prefix(ref bool __result)
	{
		if (AmongUsClient.Instance == null ||
			AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
			AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
		{
			__result = false;
			return false;
		}
		return true;
	}
}
*/