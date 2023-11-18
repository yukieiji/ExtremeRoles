#if DEBUG
using HarmonyLib;

// From Reactor.RemoveAccounts by MIT License : https://github.com/NuclearPowered/Reactor.RemoveAccounts

namespace ExtremeRoles.Patches.DebugPatch.RemoveAccount
{
    [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
    public static class AccountManagerCanPlayOnlinePatch
    {
        public static bool Prefix(out bool __result)
        {
            __result = true;
            return false;
        }
    }

	[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.OnSceneLoaded))]
	public static class AccountManagerOnSceneLoadedPatch
	{
		public static void Postfix(AccountManager __instance)
		{
			__instance.privacyPolicyBg.SetActive(false);
			__instance.waitingText.SetActive(false);
			__instance.postLoadWaiting.SetActive(false);
		}
	}
}
#endif
