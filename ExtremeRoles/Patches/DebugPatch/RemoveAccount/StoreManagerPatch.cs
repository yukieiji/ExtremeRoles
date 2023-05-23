#if DEBUG
using HarmonyLib;

// From Reactor.RemoveAccounts by MIT License : https://github.com/NuclearPowered/Reactor.RemoveAccounts

namespace ExtremeRoles.Patches.DebugPatch.RemoveAccount
{
    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.Initialize))]
    public static class StoreManagerPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
#endif
