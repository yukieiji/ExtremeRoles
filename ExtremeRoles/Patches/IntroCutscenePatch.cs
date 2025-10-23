using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutsceneBeginImpostorPatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
		if (__instance.TryGetComponent<IntroCutsceneModder>(out var mod))
		{
			mod.BeginImpostorPrefix(__instance, ref yourTeam);
		}
	}

	public static void Postfix(
		IntroCutscene __instance)
	{
		if (__instance.TryGetComponent<IntroCutsceneModder>(out var mod))
		{
			mod.BeginImpostorPostfix(__instance);
		}
	}
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class BeginCrewmatePatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
		if (__instance.TryGetComponent<IntroCutsceneModder>(out var mod))
		{
			mod.BeginCrewmatePrefix(__instance, ref teamToDisplay);
		}
	}

    public static void Postfix(
        IntroCutscene __instance)
    {
		if (__instance.TryGetComponent<IntroCutsceneModder>(out var mod))
		{
			mod.BeginCrewmatePostfix(__instance);
		}
	}
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
public static class IntroCutsceneCoBeginPatch
{
    public static bool Prefix(
        IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
		GameProgressSystem.Current = GameProgressSystem.Progress.IntroStart;

		var mod = __instance.gameObject.TryAddComponent<IntroCutsceneModder>();
		return mod.CoBeginPrefix(__instance, ref __result);
	}
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
public static class IntroCutsceneCreatePlayerPatch
{
	public static void Postfix(
		ref PoolablePlayer __result,
		[HarmonyArgument(2)] NetworkedPlayerInfo pData,
		[HarmonyArgument(3)] bool impostorPositioning)
	{
		if (!impostorPositioning ||
			VanillaRoleProvider.IsImpostorRole(pData.Role.Role))
		{
			return;
		}

		// おそらくImpostorRedであると思われるがわからないのでImpostorのデフォルトカラーを取り出す
		// 内部的にはfor分使って呼び出してるけどどうせデフォルト役職なので早めに取り出されると思われる
		__result.SetNameColor(
			RoleManager.Instance.GetRole(RoleTypes.Impostor).NameColor);
	}
}


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
public static class IntroCutsceneSetUpRoleTextPatch
{
    public static bool Prefix(
        IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
		=> __instance.TryGetComponent<IntroCutsceneModder>(out var mod) &&
			mod.ShowRolePrefix(__instance, ref __result);
}
