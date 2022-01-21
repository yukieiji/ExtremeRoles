using HarmonyLib;
using UnityEngine;

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
                __instance.TeamTitle.text = Helper.Translation.GetString("neutral");
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
            var role = ExtremeRoleManager.GetLocalPlayerRole() as IRoleSpecialSetUp;
            if (role != null)
            {
                role.IntroBeginSetUp();
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

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.SetUpRoleText))]
    class IntroCutsceneSetUpRoleTextPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (!role.IsVanillaRole())
            {
                __instance.YouAreText.color = role.NameColor;
                __instance.RoleText.text = role.GetColoredRoleName();
                __instance.RoleText.color = role.NameColor;
                __instance.RoleBlurbText.text = role.GetIntroDescription();
                __instance.RoleBlurbText.color = role.NameColor;

                if (role.Id == ExtremeRoleId.Lover) { return; }

                if (role is Roles.API.MultiAssignRoleBase)
                {
                    if (((Roles.API.MultiAssignRoleBase)role).AnotherRole != null)
                    {
                        __instance.RoleBlurbText.fontSize *= 0.50f;
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
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class IntroCutsceneOnDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {
                ExtremeRolesPlugin.Info.SetInfoButtonToInGamePositon();

                var role = ExtremeRoleManager.GetLocalPlayerRole() as IRoleSpecialSetUp;
                if (role != null)
                {
                    role.IntroEndSetUp();
                }
            }
        }
    }
}
