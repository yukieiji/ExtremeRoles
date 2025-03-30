#if DEBUG
using System.Reflection;
using System.Linq;

using AmongUs.Data;
using Epic.OnlineServices;
using HarmonyLib;
using InnerNet;

// From Reactor.RemoveAccounts by MIT License : https://github.com/NuclearPowered/Reactor.RemoveAccounts

namespace ExtremeRoles.Patches.DebugPatch.RemoveAccount
{
    [HarmonyPatch]
    public static class EOSManagerRunLoginPatch
    {

        public static MethodBase TargetMethod()
        {
            var type = typeof(EOSManager).GetNestedTypes(AccessTools.all).FirstOrDefault(
                t => t.Name.Contains(nameof(EOSManager.RunLogin)));
            return AccessTools.Method(type, nameof(Il2CppSystem.Collections.IEnumerator.MoveNext));
        }

        public static bool Prefix(ref bool __result)
        {
			EOSManager.Instance.IsAllowedOnline(true);

			__result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.Awake))]
    public static class EOSManagerAwakePatch
    {
        private static readonly PropertyInfo localUserIdProperty =
            typeof(EpicManager).GetProperty(
                "localUserId", BindingFlags.Static | BindingFlags.Public);

        public static bool Prefix(EOSManager __instance)
        {
			EOSManager._instance = __instance;
			if (__instance.DontDestroy)
			{
				UnityEngine.Object.DontDestroyOnLoad(__instance.gameObject);
			}

            __instance.platformInitialized = true;

            localUserIdProperty?.SetValue(null, new EpicAccountId());

			DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
			DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
			/*
			DataManager.Player.Onboarding.LastAcceptedPrivacyPolicyVersion =
				ReferenceDataManager.Instance.Refdata.privacyPolicyVersion;
			*/

			__instance.userId = new ProductUserId();

			__instance.hasRunLoginFlow = true;
			__instance.loginFlowFinished = true;

			return false;
        }
    }

	[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.HasFinishedLoginFlow))]
	public static class EOSManagerHasFinishedLoginFlowPatch
	{
		public static bool Prefix(out bool __result)
		{
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.ContinueInOfflineMode))]
	public static class EOSManagerContinueInOfflineModePatch
	{
		public static bool Prefix()
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.LoginWithCorrectPlatform))]
	public static class EOSManagerLoginWithCorrectPlatformPatch
	{
		public static bool Prefix()
		{
			return false;
		}
	}

	[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.ProductUserId), MethodType.Getter)]
    public static class EOSManagerProductUserIdPatch
    {
        public static bool Prefix(out string __result)
        {
            __result = string.Empty;
            return false;
        }
    }

    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.UserIDToken), MethodType.Getter)]
    public static class EOSManagerUserIDTokenPatch
    {
        public static bool Prefix(out string __result)
        {
            __result = null;
            return false;
        }
    }
}
#endif
