using System.Collections;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.GameMode.Option.MapModule;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Host;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles.Patches
{
    public static class IntroCutscenceHelper
    {

        public static void SetupIntroTeam(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            var role = ExtremeRoleManager.GetLocalPlayerRole();

            if (role.IsNeutral())
            {
                __instance.BackgroundBar.material.color = ColorPalette.NeutralColor;
                __instance.TeamTitle.text = Translation.GetString("Neutral");
                __instance.TeamTitle.color = ColorPalette.NeutralColor;
                __instance.ImpostorText.text = Translation.GetString("neutralIntro");
            }
            else if (role.Id == ExtremeRoleId.Xion)
            {
                __instance.BackgroundBar.material.color = ColorPalette.XionBlue;
                __instance.TeamTitle.text = Translation.GetString("yourHost");
                __instance.TeamTitle.color = ColorPalette.XionBlue;
                __instance.ImpostorText.text = Translation.GetString("youAreNewRuleEditor");
            }
        }

        public static void SetupIntroTeamIcons(
            IntroCutscene __instance,
            ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            // Intro solo teams
            if (role.IsNeutral() || role.Id == ExtremeRoleId.Xion)
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(CachedPlayerControl.LocalPlayer);
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
    public static class IntroCutsceneBeginImpostorPatch
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
    public static class BeginCrewmatePatch
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

            if (role.Id != ExtremeRoleId.Lover ||
                role.Id != ExtremeRoleId.Sharer ||
                role.Id != ExtremeRoleId.Buddy)
            {
                if (role is MultiAssignRoleBase)
                {
                    if (((MultiAssignRoleBase)role).AnotherRole != null)
                    {
                        __instance.RoleBlurbText.fontSize *= 0.45f;
                    }
                }


                if (role.IsImpostor())
                {
                    __instance.RoleBlurbText.text +=
                        $"\n{Translation.GetString("impostorIntroText")}";
                }
                else if (role.IsCrewmate() && role.HasTask())
                {
                    __instance.RoleBlurbText.text +=
                        $"\n{Translation.GetString("crewIntroText")}";
                }
            }

            SoundManager.Instance.PlaySound(
                CachedPlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f);

            __instance.YouAreText.gameObject.SetActive(true);
            __instance.RoleText.gameObject.SetActive(true);
            __instance.RoleBlurbText.gameObject.SetActive(true);

            if (__instance.ourCrewmate == null)
            {
                __instance.ourCrewmate = __instance.CreatePlayer(
                    0, 1, CachedPlayerControl.LocalPlayer.Data, false);
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
            var awakeVanillaRole = role as IRoleAwake<RoleTypes>;
            if (awakeVanillaRole != null && !awakeVanillaRole.IsAwake)
            {
                return true;
            }

            __result = showRoleText(role, __instance).WrapToIl2Cpp();
            return false;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    public static class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix()
        {
            if (ExtremeGameModeManager.Instance.RoleSelector.IsCanUseAndEnableXion())
            {
                Xion.XionPlayerToGhostLayer();
                Xion.RemoveXionPlayerToAllPlayerControl();

                if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
                {
                    foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
                    {
                        if (player == null) { continue; }

                        if (!player.GetComponent<DummyBehaviour>().enabled) { continue; }

                        var role = ExtremeRoleManager.GameRole[player.PlayerId];
                        if (!role.HasTask())
                        {
                            continue;
                        }

                        GameData.PlayerInfo playerInfo = player.Data;

                        var (_, totalTask) = GameSystem.GetTaskInfo(playerInfo);
                        if (totalTask == 0)
                        {
                            GameSystem.SetTask(playerInfo, 
                                GameSystem.GetRandomCommonTaskId());
                        }
                    }
                }
            }

            Module.InfoOverlay.Button.SetInfoButtonToInGamePositon();

            var localRole = ExtremeRoleManager.GetLocalPlayerRole();

            var setUpRole = localRole as IRoleSpecialSetUp;
            if (setUpRole != null)
            {
                setUpRole.IntroEndSetUp();
            }

            var multiAssignRole = localRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                setUpRole = multiAssignRole.AnotherRole as IRoleSpecialSetUp;
                if (setUpRole != null)
                {
                    setUpRole.IntroEndSetUp();
                }
            }
            disableMapObject();
        }

        private static void disableMapObject()
        {
            HashSet<string> disableObjectName = new HashSet<string>();

            var shipOpt = ExtremeGameModeManager.Instance.ShipOption;

            bool isRemoveAdmin = shipOpt.Admin.DisableAdmin;
            bool isRemoveSecurity = shipOpt.Security.DisableSecurity;
            bool isRemoveVital = shipOpt.Vital.DisableVital;

            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                var modMap = ExtremeRolesPlugin.Compat.ModMap;

                if (isRemoveAdmin)
                {
                    disableObjectName.UnionWith(
                        modMap.GetSystemObjectName(
                            Compat.Interface.SystemConsoleType.Admin));
                }
                if (isRemoveSecurity)
                {
                    disableObjectName.UnionWith(
                        modMap.GetSystemObjectName(
                            Compat.Interface.SystemConsoleType.SecurityCamera));
                }
                if (isRemoveVital)
                {
                    disableObjectName.UnionWith(
                        modMap.GetSystemObjectName(
                            Compat.Interface.SystemConsoleType.Vital));
                }
            }
            else
            {
                switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                    ByteOptionNames.MapId))
                {
                    case 0:
                        if (isRemoveAdmin)
                        {
                            disableObjectName.Add(
                                GameSystem.SkeldAdmin);
                        }
                        if (isRemoveSecurity)
                        {
                            disableObjectName.Add(
                                GameSystem.SkeldSecurity);
                        }
                        break;
                    case 1:
                        if (isRemoveAdmin)
                        {
                            disableObjectName.Add(
                                GameSystem.MiraHqAdmin);
                        }
                        if (isRemoveSecurity)
                        {
                            disableObjectName.Add(
                                GameSystem.MiraHqSecurity);
                        }
                        break;
                    case 2:
                        if (isRemoveAdmin)
                        {
                            disableObjectName.Add(
                                GameSystem.PolusAdmin1);
                            disableObjectName.Add(
                                GameSystem.PolusAdmin2);
                        }
                        if (isRemoveSecurity)
                        {
                            disableObjectName.Add(
                                GameSystem.PolusSecurity);
                        }
                        if (isRemoveVital)
                        {
                            disableObjectName.Add(
                                GameSystem.PolusVital);
                        }
                        break;
                    case 4:
                        if (isRemoveAdmin)
                        {
                            disableObjectName.Add(
                                GameSystem.AirShipArchiveAdmin);
                            disableObjectName.Add(
                                GameSystem.AirShipCockpitAdmin);
                        }
                        else
                        {
                            switch (shipOpt.Admin.AirShipEnable)
                            {
                                case AirShipAdminMode.ModeCockpitOnly:
                                    disableObjectName.Add(
                                        GameSystem.AirShipArchiveAdmin);
                                    break;
                                case AirShipAdminMode.ModeArchiveOnly:
                                    disableObjectName.Add(
                                        GameSystem.AirShipCockpitAdmin);
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (isRemoveSecurity)
                        {
                            disableObjectName.Add(
                                GameSystem.AirShipSecurity);
                        }
                        if (isRemoveVital)
                        {
                            disableObjectName.Add(
                                GameSystem.AirShipVital);
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (string objectName in disableObjectName)
            {
                GameSystem.DisableMapModule(objectName);
            }
        }
    }
}
