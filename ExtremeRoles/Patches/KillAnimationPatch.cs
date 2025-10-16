using System.Collections.Generic;

using HarmonyLib;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Patches;

public sealed class KillAnimationHook
{
	private readonly List<IRolePerformKillHook> hooks = new List<IRolePerformKillHook>(2);

	public KillAnimationHook(PlayerControl source, PlayerControl target)
	{
		this.hooks.Clear();

		var localPc = PlayerControl.LocalPlayer;
		if (!(
				GameProgressSystem.IsGameNow &&
				localPc != null &&
				(source.PlayerId == localPc.PlayerId || target.PlayerId == localPc.PlayerId)
			))
		{
			return;
		}

		var (main, sub) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRolePerformKillHook>();
		if (main is null && sub is null)
		{
			return;
		}

		if (main is not null)
		{
			this.hooks.Add(main);
		}
		if (sub is not null)
		{
			this.hooks.Add(sub);
		}
	}

	public void StartKill()
	{
		foreach (var h in hooks)
		{
			h.OnStartKill();
		}
	}
	public void EndKill()
	{
		foreach (var h in hooks)
		{
			h.OnEndKill();
		}
		hooks.Clear();
	}
}

[HarmonyPatch(typeof(KillAnimation._CoPerformKill_d__2), nameof(KillAnimation._CoPerformKill_d__2.MoveNext))]
public static class KillAnimationCoPerformKillMoveNextPatch
{
    public static bool HideNextAnimation = false;
	private static KillAnimationHook? hook;

    public static void Prefix(
        KillAnimation._CoPerformKill_d__2 __instance)
    {
		if (hook is null)
		{
			hook = new KillAnimationHook(__instance.source, __instance.target);
			hook.StartKill();
		}

        if (HideNextAnimation)
        {
            __instance.source = __instance.target;
        }
        HideNextAnimation = false;
    }
	public static void Postfix(
		KillAnimation._CoPerformKill_d__2 __instance, ref bool __result)
	{
		if (__result || hook is null)
		{
			return;
		}
		hook.EndKill();
		hook = null;
	}
}