using HarmonyLib;
using Hazel.Udp;

namespace ExtremeRoles.Patches
{

    // from TOR-GM-H : https://github.com/haoming37/TheOtherRoles-GM-Haoming/commit/70a5e7ebcc55a42ecd4042cd17fa5e98566f8dbb
    [HarmonyPatch(typeof(UnityUdpClientConnection), nameof(UnityUdpClientConnection.ConnectAsync))]
    public static class UnityUdpClientConnectionConnectAsyncPatch
    {
        public static void Prefix(UnityUdpClientConnection __instance)
        {
            __instance.KeepAliveInterval = 2000;
            __instance.DisconnectTimeoutMs = 15000;
        }
    }
}
