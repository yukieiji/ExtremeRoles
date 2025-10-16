using ExtremeRoles.GameMode;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using HarmonyLib;

using UnityEngine;

namespace ExtremeRoles.Patches.MiniGame;


[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class SpawnInMinigameBeginPatch
{
	public static void Prefix()
	{
		GameProgressSystem.Current = GameProgressSystem.Progress.PreTask;
	}

	public static void Postfix(SpawnInMinigame __instance)
    {
		var spawnOpt = ExtremeGameModeManager.Instance.ShipOption.Spawn;

		if (!(spawnOpt.EnableSpecialSetting && spawnOpt.AirShip))
		{
			__instance.gotButton = true;

			PlayerControl localPlayer = PlayerControl.LocalPlayer;

			localPlayer.SetKinematic(true);
			localPlayer.NetTransform.SetPaused(true);
			Helper.Player.RpcUncheckSnap(localPlayer.PlayerId, new Vector2(-0.66f, -0.5f));
			HudManager.Instance.PlayerCam.SnapToTarget();

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

