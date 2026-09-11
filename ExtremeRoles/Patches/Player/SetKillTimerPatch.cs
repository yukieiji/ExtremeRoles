using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;
using PlayerHelper = ExtremeRoles.Helper.Player;

namespace ExtremeRoles.Patches.Player;

#nullable enable


public class PlayerControlSetKillTimerPatchBody(IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;

	public bool Prefix(PlayerControl __instance, float time)
	{
		if (!(
				_progress.IsTaskPhase && 
				_runtime.TryGetGameContext(out var ctx) && 
				ctx.Roles.TryGetRole(__instance.PlayerId, out var role)
			))
		{
			return true;
		}

		float killCool = PlayerHelper.DefaultKillCoolTime;
		if (killCool <= 0f)
		{
			return false;
		}

		float maxTime = role.TryGetKillCool(out float otherKillCool) ? otherKillCool : killCool;
		__instance.killTimer = Mathf.Clamp(
			time, 0f, maxTime);
		HudManager.Instance.KillButton.SetCoolDown(__instance.killTimer, maxTime);
		
		return false;
	}
}



[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControlSetKillTimernPatch
{
	private static PlayerControlSetKillTimerPatchBody? _body;

	public static bool Prefix(
		PlayerControl __instance, [HarmonyArgument(0)] float time)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<PlayerControlSetKillTimerPatchBody>();
		}
		return _body.Prefix(__instance, time);
	}
}
