using System.Collections;

using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches;

public static class IntroCutscenceHelper
{

    public static void SetupIntroTeam(
        IntroCutscene __instance)
    {
        var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (role.IsNeutral())
        {
			var (main, sub) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>();
			if ((main is not null && !main.IsAwake) || (sub is not null && !sub.IsAwake))
			{
				return;
			}

			__instance.BackgroundBar.material.color = ColorPalette.NeutralColor;
            __instance.TeamTitle.text = Tr.GetString("Neutral");
            __instance.TeamTitle.color = ColorPalette.NeutralColor;
            __instance.ImpostorText.text = Tr.GetString("neutralIntro");
        }
        else if (role.Id == ExtremeRoleId.Xion)
        {
            __instance.BackgroundBar.material.color = ColorPalette.XionBlue;
            __instance.TeamTitle.text = Tr.GetString("yourHost");
            __instance.TeamTitle.color = ColorPalette.XionBlue;
            __instance.ImpostorText.text = Tr.GetString("youAreNewRuleEditor");
        }
    }

    public static void SetupIntroTeamIcons(
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {

        var role = ExtremeRoleManager.GetLocalPlayerRole();

        // Intro solo teams
        if (role.IsNeutral() || role.Id == ExtremeRoleId.Xion)
        {
			var (main, sub) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>();
			if ((main is not null && !main.IsAwake) || (sub is not null && !sub.IsAwake))
			{
				return;
			}

			var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            soloTeam.Add(PlayerControl.LocalPlayer);
            yourTeam = soloTeam;
        }
    }

    public static void SetupPlayerPrefab(IntroCutscene __instance)
    {
        Prefab.PlayerPrefab = Object.Instantiate(
            __instance.PlayerPrefab);
        Object.DontDestroyOnLoad(Prefab.PlayerPrefab);
        Prefab.PlayerPrefab.name = "poolablePlayerPrefab";
        Prefab.PlayerPrefab.gameObject.SetActive(false);
    }

    public static void SetupRole()
    {
		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (localRole is IRoleSpecialSetUp setUpRole)
		{
			setUpRole.IntroBeginSetUp();
		}

		if (localRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleSpecialSetUp multiSetUpRole)
		{
			multiSetUpRole.IntroBeginSetUp();
		}
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutsceneBeginImpostorPatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        IntroCutscenceHelper.SetupIntroTeamIcons(ref yourTeam);
        IntroCutscenceHelper.SetupPlayerPrefab(__instance);
    }

    public static void Postfix(
        IntroCutscene __instance)
    {
        IntroCutscenceHelper.SetupIntroTeam(__instance);
        IntroCutscenceHelper.SetupRole();
    }

}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class BeginCrewmatePatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        IntroCutscenceHelper.SetupIntroTeamIcons(ref teamToDisplay);
        IntroCutscenceHelper.SetupPlayerPrefab(__instance);
    }

    public static void Postfix(
        IntroCutscene __instance)
    {
        IntroCutscenceHelper.SetupIntroTeam(__instance);
        IntroCutscenceHelper.SetupRole();
    }
}
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
public static class IntroCutsceneCoBeginPatch
{
    public static bool Prefix(
        IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        IIntroRunner runnner = ExtremeGameModeManager.Instance.GetIntroRunner();
        if (runnner == null) { return true; }

        __result = runnner.CoRunIntro(__instance).WrapToIl2Cpp();
        return false;
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
public static class IntroCutsceneSetUpRoleTextPatch
{
    private static IEnumerator showRoleText(
        SingleRoleBase role,
        IntroCutscene __instance)
    {
        __instance.YouAreText.color = role.GetNameColor();
        __instance.RoleText.text = role.GetColoredRoleName();
        __instance.RoleText.color = role.GetNameColor();
        __instance.RoleBlurbText.text = role.GetIntroDescription();
        __instance.RoleBlurbText.color = role.GetNameColor();

        if (role.Id is ExtremeRoleId.Lover
			or ExtremeRoleId.Sharer
			or ExtremeRoleId.Buddy)
        {
            if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole != null)
            {
				__instance.RoleBlurbText.fontSize *= 0.45f;
			}


            if (role.IsImpostor())
            {
                __instance.RoleBlurbText.text +=
                    $"\n{Tr.GetString("impostorIntroText")}";
            }
            else if (role.IsCrewmate() && role.HasTask())
            {
                __instance.RoleBlurbText.text +=
                    $"\n{Tr.GetString("crewIntroText")}";
            }
        }

        SoundManager.Instance.PlaySound(
            PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f);

        __instance.YouAreText.gameObject.SetActive(true);
        __instance.RoleText.gameObject.SetActive(true);
        __instance.RoleBlurbText.gameObject.SetActive(true);

        if (__instance.ourCrewmate == null)
        {
            __instance.ourCrewmate = __instance.CreatePlayer(
                0, 1, PlayerControl.LocalPlayer.Data, false);
            __instance.ourCrewmate.gameObject.SetActive(false);
        }
        __instance.ourCrewmate.gameObject.SetActive(true);
        __instance.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
        __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);

        yield return new WaitForSeconds(2.5f);

        __instance.YouAreText.gameObject.SetActive(false);
        __instance.RoleText.gameObject.SetActive(false);
        __instance.RoleBlurbText.gameObject.SetActive(false);
        __instance.ourCrewmate.gameObject.SetActive(false);

        yield break;
    }

    public static bool Prefix(
        IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        var role = ExtremeRoleManager.GetLocalPlayerRole();
        if (role.IsVanillaRole() ||
			(role is IRoleAwake<RoleTypes> awakeVanillaRole && !awakeVanillaRole.IsAwake))
		{
			return true;
		}

        __result = showRoleText(role, __instance).WrapToIl2Cpp();
        return false;
    }
}
