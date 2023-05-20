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
    public static void Postfix(ref Il2CppStructArray<byte> __result)
    {
        var serverMng = FastDestroyableSingleton<ServerManager>.Instance;

        if (serverMng == null || !serverMng.IsCustomServer())
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
