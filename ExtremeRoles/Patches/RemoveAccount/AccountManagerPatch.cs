#if DEBUG
using HarmonyLib;

// From Reactor.RemoveAccounts by MIT License : https://github.com/NuclearPowered/Reactor.RemoveAccounts

namespace ExtremeRoles.Patches.RemoveAccount
{
    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
    public static class CanPlayOnlinePatch
    {
        public static bool Prefix(out bool __result)
        {
            __result = true;
            return false;
        }
    }
}
#endif
