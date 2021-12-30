using HarmonyLib;

using ExtremeRoles.Roles;

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
                __instance.ImpostorText.text = string.Format(Helper.Translation.GetString("neutralIntro"));
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

    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class IntroCutsceneBeginImpostorPatch
    {
        public static void Prefix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroCutscenceHelper.SetupIntroTeamIcons(__instance, ref yourTeam);
        }

        public static void Postfix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroCutscenceHelper.SetupIntroTeam(__instance, ref yourTeam);
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
        }

        public static void Postfix(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            IntroCutscenceHelper.SetupIntroTeam(__instance, ref yourTeam);
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
                
                if (role is Roles.API.MultiAssignRoleBase)
                {
                    if (((Roles.API.MultiAssignRoleBase)role).AnotherRole != null)
                    {
                        __instance.RoleBlurbText.fontSize *= 0.50f;
                    }
                }

                if (role.Team == Roles.API.ExtremeRoleType.Impostor)
                {
                    __instance.RoleBlurbText.text += string.Format(
                        "\n{0}", Helper.Translation.GetString("impostorIntroText"));
                }
                else if(role.Team == Roles.API.ExtremeRoleType.Crewmate)
                {
                    __instance.RoleBlurbText.text += string.Format(
                        "\n{0}", Helper.Translation.GetString("crewIntroText"));
                }

            }
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
            if (PlayerControl.LocalPlayer != null && HudManager.Instance != null)
            {
                Module.GameDataContainer.CreatIcons(__instance);
                var role = ExtremeRoleManager.GetLocalPlayerRole();
                if (role.Id == ExtremeRoleId.Marlin)
                {
                    ((Roles.Combination.Marlin)role).SetPlayerIcon(
                        Module.GameDataContainer.PlayerIcon);
                }
            }

        }
    }
}
