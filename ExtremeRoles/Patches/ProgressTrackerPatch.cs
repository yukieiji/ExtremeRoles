using System.Linq;

using AmongUs.GameOptions;
using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Patches;

public class ProgressTrackerFixedUpdatePatchBody(IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;

	public bool Prefix(ProgressTracker __instance)
		=>
			GameManager.Instance != null &&
			GameManager.Instance.LogicOptions != null &&
			__instance.TileParent != null &&
			PlayerControl.LocalPlayer != null;

	public void Postfix(ProgressTracker __instance)
	{
		if (!(
				_runtime.TryGetGameContext(out var ctx) &&
				_progress.IsGameNow && (
				(
					ctx.Roles.TryGetSafeCastedLocalRole<Agency>(out var agency) &&
					agency.CanSeeTaskBar
				) ||
				(
					ctx.Roles.TryGetSafeCastedLocalRole<SlaveDriver>(out var slaveDriver) &&
					slaveDriver.CanSeeTaskBar
				))
			))
		{
			return;
		}

		if (!__instance.TileParent.enabled)
		{
			__instance.TileParent.enabled = true;
		}
		
		GameData gameData = GameData.Instance;
		
		if (gameData == null || gameData.TotalTasks <= 0)
		{
			return;
		}

		__instance.gameObject.SetActive(true);
		int num = (TutorialManager.InstanceExists ?
			1 : (gameData.PlayerCount - GameOptionsManager.Instance.CurrentGameOptions.GetInt(
					Int32OptionNames.NumImpostors)));
		num -= gameData.AllPlayers.ToArray().ToList().Count(
			(NetworkedPlayerInfo p) => p.Disconnected);
		float curProgress = (float)gameData.CompletedTasks /
			(float)gameData.TotalTasks * (float)num;
		__instance.curValue = Mathf.Lerp(
			__instance.curValue, curProgress, Time.fixedDeltaTime * 2f);
		__instance.TileParent.material.SetFloat("_Buckets", (float)num);
		__instance.TileParent.material.SetFloat("_FullBuckets", __instance.curValue);
	}
}


[HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.FixedUpdate))]
public static class ProgressTrackerFixedUpdatePatch
{
	private static ProgressTrackerFixedUpdatePatchBody? _body;

	public static bool Prefix(ProgressTracker __instance)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ProgressTrackerFixedUpdatePatchBody>();
		}
		return _body.Prefix(__instance);
	}


	public static void Postfix(ProgressTracker __instance)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<ProgressTrackerFixedUpdatePatchBody>();
		}
		_body.Postfix(__instance);
	}

}
