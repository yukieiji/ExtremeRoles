using HarmonyLib;



namespace ExtremeRoles.Patches
{
    class IntroCutscenceHelper
    {
        public static void SetupIntroTeam(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            var role = Roles.ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];

            if (role.IsNeutral())
            {
                __instance.BackgroundBar.material.color = Modules.ColorPalette.NeutralColor;
                __instance.TeamTitle.text = role.Teams.ToString();
                __instance.TeamTitle.color = Modules.ColorPalette.NeutralColor;
                __instance.ImpostorText.text = "";
            }
        }

        public static void SetupIntroTeamIcons(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {

            var role = Roles.ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];

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
            var role = Roles.ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];

            if (role.Id != Roles.ExtremeRoleId.VanillaRole)
            {
                __instance.YouAreText.color = role.NameColor;
                __instance.RoleText.text = role.RoleName;
                __instance.RoleText.color = role.NameColor;
                __instance.RoleBlurbText.text = string.Format(
                    "{0}{1}", role.Id, "IntroDescription");
                __instance.RoleBlurbText.color = role.NameColor;
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
                Modules.PlayerDataContainer.CreatIcons(__instance);
                var role = Roles.ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];
                if (role.Id == Roles.ExtremeRoleId.Marlin)
                {
                    ((Roles.Combination.Marlin)role).SetPlayerIcon(
                        Modules.PlayerDataContainer.PlayerIcon);
                }
            }

        }
    }
}
