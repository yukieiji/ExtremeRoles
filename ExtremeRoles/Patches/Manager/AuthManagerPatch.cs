using Il2CppSystem;
using Hazel;
using HarmonyLib;

using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
public static class AuthManagerCoConnectPatch
{
	public static bool Prefix(AuthManager._CoConnect_d__4 __instance)
	{
		if (!ServerManager.Instance.IsCustomServer())
		{
			return true;
		}

		var instance = __instance.__4__this;
		if (instance.connection != null)
		{
			instance.connection.DataReceived -=
				(Action<DataReceivedEventArgs>)instance.Connection_DataReceived;
			instance.connection.Disconnected -=
				(EventHandler<DisconnectedEventArgs>)instance.Connection_Disconnected;
			instance.connection.Dispose();
		}

		instance.connection = null;

		return false;
	}
}
