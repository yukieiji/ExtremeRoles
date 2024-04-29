using ExtremeRoles.GameMode;
using ExtremeRoles.Performance;
using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches.MiniGame;


[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class SpawnInMinigameBeginPatch
{
    public static void Postfix(SpawnInMinigame __instance)
    {
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		if (spawnOpt == null) { return; }

		if (!(spawnOpt.EnableSpecialSetting && spawnOpt.AirShip))
		{
			__instance.gotButton = true;

			PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

			localPlayer.SetKinematic(true);
			localPlayer.NetTransform.SetPaused(true);
			Helper.Player.RpcUncheckSnap(localPlayer.PlayerId, new Vector2(-0.66f, -0.5f));
			DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();

			__instance.StopAllCoroutines();
			__instance.StartCoroutine(
				__instance.CoSpawnAt(
					localPlayer,
					new SpawnInMinigame.SpawnLocation()));
		}
        else if (spawnOpt.IsAutoSelectRandom)
        {
            __instance.Close();
        }
    }
}
