using Il2CppSystem;
using Hazel;
using HarmonyLib;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(AuthManager), nameof(AuthManager.CoConnect))]
public static class AuthManagerCoConnectPatch
{
	public static bool Prefix(AuthManager __instance)
	{
		if (!FastDestroyableSingleton<ServerManager>.Instance.IsCustomServer())
		{
			return true;
		}

		if (__instance.connection != null)
		{
			__instance.connection.DataReceived -=
				(Action<DataReceivedEventArgs>)__instance.Connection_DataReceived;
			__instance.connection.Disconnected -=
				(EventHandler<DisconnectedEventArgs>)__instance.Connection_Disconnected;
			__instance.connection.Dispose();
		}

		__instance.connection = null;

		return false;
	}
}
