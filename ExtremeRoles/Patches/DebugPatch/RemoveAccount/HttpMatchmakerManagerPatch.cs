#if DEBUG

using System;
using System.Text.Json;

using HarmonyLib;

// From Reactor.RemoveAccounts by MIT License : https://github.com/NuclearPowered/Reactor.RemoveAccounts

namespace ExtremeRoles.Patches.DebugPatch.RemoveAccount
{
    [HarmonyPatch(typeof(HttpMatchmakerManager), nameof(HttpMatchmakerManager.TryReadCachedToken))]
    public static class HttpMatchmakerManagerTryReadCachedTokenPatch
    {
        public static bool Prefix(out bool __result, out string matchmakerToken)
        {

			__result = true;
			matchmakerToken = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(new
			{
				Content = new
				{
					Puid = "RemoveAccounts",
					ClientVersion = Constants.GetBroadcastVersion(),
					ExpiresAt = DateTime.UtcNow.AddHours(1),
				},
				Hash = "RemoveAccounts",
			}));
			return false;
        }
    }
}
#endif
