using System.Collections;

using HarmonyLib;
using UnityEngine;

using BepInEx.IL2CPP.Utils.Collections;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches
{
    class IntroCutscenceHelper
    {

        public static void SetupIntroTeam(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (role.IsNeutral())
            {
                __instance.BackgroundBar.material.color = Module.ColorPalette.NeutralColor;
                __instance.TeamTitle.text = Helper.Translation.GetString("Neutral");
                __instance.TeamTitle.color = Module.ColorPalette.NeutralColor;
                __instance.ImpostorText.text = Helper.Translation.GetString("neutralIntro");
            }
        }

        public static void SetupIntroTeamIcons(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            // Intro solo teams
            if (role.IsNeutral())
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }
        }

        public static void SetupPlayerPrefab(IntroCutscene __instance)
        {
            Module.Prefab.PlayerPrefab = UnityEngine.Object.Instantiate(
                __instance.PlayerPrefab);
            UnityEngine.Object.DontDestroyOnLoad(Module.Prefab.PlayerPrefab);
            Module.Prefab.PlayerPrefab.name = "poolablePlayerPrefab";
            Module.Prefab.PlayerPrefab.gameObject.SetActive(false);
        }

        public static void SetupRole()
        {
            var localRole = ExtremeRoleManager.GetLocalPlayerRole();

            var setUpRole = localRole as IRoleSpecialSetUp;
            if (setUpRole != null)
            {
                setUpRole.IntroBeginSetUp();
            }

            var multiAssignRole = localRole as Roles.API.MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                setUpRole = multiAssignRole.AnotherRole as IRoleSpecialSetUp;
                if (setUpRole != null)
                {
                    setUpRole.IntroBeginSetUp();
                }
            }
        }

    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class IntroCutsceneBeginImpostorPatch
    {
        public static void Prefix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroCutscenceHelper.SetupIntroTeamIcons(__instance, ref yourTeam);
            IntroCutscenceHelper.SetupPlayerPrefab(__instance);
        }

        public static void Postfix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroCutscenceHelper.SetupIntroTeam(__instance, ref yourTeam);
            IntroCutscenceHelper.SetupRole();
        }

    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            IntroCutscenceHelper.SetupIntroTeamIcons(__instance, ref teamToDisplay);
            IntroCutscenceHelper.SetupPlayerPrefab(__instance);
        }

        public static void Postfix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            IntroCutscenceHelper.SetupIntroTeam(__instance, ref teamToDisplay);
            IntroCutscenceHelper.SetupRole();
        }
    }
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class IntroCutsceneSetUpRoleTextPatch
    {
        private static IEnumerator showRoleText(
            Roles.API.SingleRoleBase role,
            IntroCutscene __instance)
        {
            __instance.YouAreText.color = role.GetNameColor();
            __instance.RoleText.text = role.GetColoredRoleName();
            __instance.RoleText.color = role.GetNameColor();
            __instance.RoleBlurbText.text = role.GetIntroDescription();
            __instance.RoleBlurbText.color = role.GetNameColor();

            if (role.Id != ExtremeRoleId.Lover)
            {
                if (role is Roles.API.MultiAssignRoleBase)
                {
                    if (((Roles.API.MultiAssignRoleBase)role).AnotherRole != null)
                    {
                        __instance.RoleBlurbText.fontSize *= 0.45f;
                    }
                }


                if (role.IsImpostor())
                {
                    __instance.RoleBlurbText.text +=
                        $"\n{Helper.Translation.GetString("impostorIntroText")}";
                }
                else if (role.IsCrewmate() && role.HasTask)
                {
                    __instance.RoleBlurbText.text +=
                        $"\n{Helper.Translation.GetString("crewIntroText")}";
                }
            }

            SoundManager.Instance.PlaySound(
                PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f);

            __instance.YouAreText.gameObject.SetActive(true);
            __instance.RoleText.gameObject.SetActive(true);
            __instance.RoleBlurbText.gameObject.SetActive(true);

            if (__instance.ourCrewmate == null)
            {
                __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
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
            if (role.IsVanillaRole()) { return true; }
            __result = showRoleText(role, __instance).WrapToIl2Cpp();
            return false;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            ExtremeRolesPlugin.Info.SetInfoButtonToInGamePositon();

            var localRole = ExtremeRoleManager.GetLocalPlayerRole();

            var setUpRole = localRole as IRoleSpecialSetUp;
            if (setUpRole != null)
            {
                setUpRole.IntroEndSetUp();
            }

            var multiAssignRole = localRole as Roles.API.MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                setUpRole = multiAssignRole.AnotherRole as IRoleSpecialSetUp;
                if (setUpRole != null)
                {
                    setUpRole.IntroEndSetUp();
                }
            }
        }
    }
}
