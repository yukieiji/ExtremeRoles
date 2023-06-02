using System.Reflection;

using HarmonyLib;
using Hazel;

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;

using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.GetConnectionData))]
public static class InnerNetClientGetConnectionDataPatch
{
	// FromReactor
	public static void Prefix(ref bool useDtlsLayout)
	{
		// Due to reasons currently unknown, the useDtlsLayout parameter sometimes doesn't reflect whether DTLS
		// is actually supposed to be enabled. This causes a bad handshake message and a quick disconnect.
		// The field on AmongUsClient appears to be more reliable, so override this parameter with what it is supposed to be.
		useDtlsLayout = AmongUsClient.Instance.useDtls;
	}

	[HarmonyPostfix, HarmonyPriority(Priority.First)]
    public static void Postfix(ref Il2CppStructArray<byte> __result)
    {
        var serverMng = FastDestroyableSingleton<ServerManager>.Instance;

        if (serverMng == null || !serverMng.IsExROnlyServer())
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var ver = assembly.GetName().Version;
        string name = assembly.GetName().Name;

        var handshake = new MessageWriter(1000);
        handshake.Write(__result);
        handshake.Write(name);
        handshake.Write(ver.Major);
        handshake.Write(ver.Minor);
        handshake.Write(ver.Build);
        handshake.Write(ver.Revision);

        __result = handshake.ToByteArray(true);
        handshake.Recycle();
    }
}
