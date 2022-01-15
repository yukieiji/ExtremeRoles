using HarmonyLib;

using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using static UnityEngine.UI.Button;

namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudManagerStartPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static void PostFix(HudManager __instance)
        {

            if (ExtremeRolesPlugin.RoleHelpButton == null)
            {
                createHelpButton();
            }
            else
            {
                setGameStartShipButtonPositon();
            }
        }
        private static void setGameStartShipButtonPositon()
        {
            ExtremeRolesPlugin.RoleHelpButton.transform.localPosition = new Vector3(
                4.925f, 2.0f, 0.0f);
        }

        private static void createHelpButton()
        {
            var topRight = GameObject.Find("TopRight");

            ExtremeRolesPlugin.RoleHelpButton = Object.Instantiate(
                Module.Prefab.HelpButton,
                topRight.transform);
            Object.DontDestroyOnLoad(ExtremeRolesPlugin.RoleHelpButton);
            ExtremeRolesPlugin.RoleHelpButton.name = "infoRoleButton";
            ExtremeRolesPlugin.RoleHelpButton.gameObject.SetActive(true);
            ExtremeRolesPlugin.RoleHelpButton.layer = 5;
            setGameStartShipButtonPositon();
            var passiveButton = ExtremeRolesPlugin.RoleHelpButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnClick.AddListener(
                (UnityAction)ExtremeRolesPlugin.Info.ToggleInfoOverlay);
        }

    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdatePatch
    {
        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings != null)
            {
                __instance.GameSettings.fontSize = 1.2f;
            }
            if (ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {

                __instance.UseButton.ToggleVisible(false);
                __instance.AbilityButton.ToggleVisible(false);
                __instance.ReportButton.ToggleVisible(false);
                __instance.KillButton.ToggleVisible(false);
                __instance.SabotageButton.ToggleVisible(false);
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.TaskText.transform.parent.gameObject.SetActive(false);
                __instance.roomTracker.gameObject.SetActive(false);
                
                IVirtualJoystick virtualJoystick = __instance.joystick;

                if (virtualJoystick != null)
                {
                    virtualJoystick.ToggleVisuals(false);
                }
            }

        }
        public static void Postfix(HudManager __instance)
        {
            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd()) { return; }

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            
            var role = ExtremeRoleManager.GetLocalPlayerRole();

            buttonCreate(role);
            roleUpdate(role);

            
            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    buttonCreate(multiAssignRole.AnotherRole);
                    roleUpdate(multiAssignRole.AnotherRole);
                }
            }
            

        }
        private static void buttonCreate(SingleRoleBase checkRole)
        {
            var abilityRole = checkRole as IRoleAbility;

            if (abilityRole != null)
            {
                if (abilityRole.Button == null)
                {
                    abilityRole.CreateAbility();
                    abilityRole.RoleAbilityInit();
                }
            }
        }

        private static void roleUpdate(SingleRoleBase checkRole)
        {
            var updatableRole = checkRole as IRoleUpdate;
            if (updatableRole != null)
            {
                updatableRole.Update(PlayerControl.LocalPlayer);
            }
        }

    }
}
