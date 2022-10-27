#if DEBUG
using HarmonyLib;

// From Reactor.RemoveAccounts by MIT License : https://github.com/NuclearPowered/Reactor.RemoveAccounts

namespace ExtremeRoles.Patches.RemoveAccount
{
    [HarmonyPatch(typeof(FriendsListButton), nameof(FriendsListButton.Awake))]
    public static class FriendsListDestroy
    {
        public static void Prefix(FriendsListButton __instance)
        {
            UnityEngine.Object.Destroy(__instance.gameObject);
        }
    }
}
#endif
