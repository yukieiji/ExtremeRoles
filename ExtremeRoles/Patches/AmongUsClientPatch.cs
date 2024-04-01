using HarmonyLib;

namespace ExtremeRoles.Patches;

[HarmonyPatch]
public static class SyncSettingPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    public static void Postfix()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.LogicOptions.SyncOptions();
        }
    }
}

// from Reactor : https://github.com/NuclearPowered/Reactor/commit/0a03a9d90d41b3bb158fa95bb23186f6769e0f9f
[HarmonyPatch(typeof(AmongUsClient._CoJoinOnlinePublicGame_d__1),
    nameof(AmongUsClient._CoJoinOnlinePublicGame_d__1.MoveNext))]
public static class EnableUdpMatchmakingPatch
{
    public static void Prefix(
        AmongUsClient._CoJoinOnlinePublicGame_d__1 __instance)
    {
        // Skip to state 1 which just calls CoJoinOnlineGameDirect
        if (__instance.__1__state == 0 && !ServerManager.Instance.IsHttp)
        {
            __instance.__1__state = 1;
            __instance.__8__1 = new AmongUsClient.__c__DisplayClass1_0
            {
                matchmakerToken = string.Empty,
            };
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
public static class AmongUsClientAwakePatch
{
	public static void Prefix(AmongUsClient __instance)
	{
		// MODなので待ち時間をとりあえず +10秒しておく、デフォルトは10秒
		__instance.MAX_CLIENT_WAIT_TIME = 20;
	}
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClientCoStartGamePatch
{
    public static void Prefix()
    {
		InfoOverlay.Instance.Hide();
	}
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class AmongUsClientOnPlayerJoinedPatch
{
    public static void Postfix()
    {
        if (PlayerControl.LocalPlayer != null)
        {
            Helper.GameSystem.ShareVersion();
        }
    }
}

// ゲームが終了した瞬間の処理
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class AmongUsClientOnGameEndPatch
{
    public static void Prefix([HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
		InfoOverlay.Instance.Hide();
		ExtremeRolesPlugin.ShipState.SetGameOverReason(endGameResult.GameOverReason);
        if ((int)endGameResult.GameOverReason >= 20)
        {
            endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
        }
    }
}
