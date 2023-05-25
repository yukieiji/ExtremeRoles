using HarmonyLib;
using Hazel.Udp;
using Hazel.Dtls;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Patches;

// from TOR-GM-H : https://github.com/haoming37/TheOtherRoles-GM-Haoming/commit/70a5e7ebcc55a42ecd4042cd17fa5e98566f8dbb
[HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.ConnectAsync))]
public static class UnityUdpClientConnectionConnectAsyncPatch
{
    public static void Prefix(UnityUdpClientConnection __instance)
    {
		// Ignore Auth when Custom servers, for set timeout 1ms
		if (__instance.TryCast<DtlsUnityConnection> != null &&
			FastDestroyableSingleton<ServerManager>.Instance.IsCustomServer())
		{
			__instance.DisconnectTimeoutMs = 1;
			return;
		}

        __instance.KeepAliveInterval = 2000;
        __instance.DisconnectTimeoutMs = 15000;
    }
}
