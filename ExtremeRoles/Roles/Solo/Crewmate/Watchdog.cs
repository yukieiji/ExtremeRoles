using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Watchdog : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public Watchdog() : base(
            ExtremeRoleId.Watchdog,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Watchdog.ToString(),
            ColorPalette.WatchdogViolet,
            false, true, false, false)
        { }

        public RoleAbilityButtonBase Button
        { 
            get => this.securityButton;
            set
            {
                this.securityButton = value;
            }
        }

        private RoleAbilityButtonBase securityButton;
        private Minigame monitoring;
        private TMPro.TextMeshPro chargeTime;

        public void CleanUp()
        {
            if (this.monitoring != null)
            {
                this.monitoring.Close();
            }
        }

        public void CreateAbility()
        {

            string buttonText;
            Sprite buttonImage;

            switch (PlayerControl.GameOptions.MapId)
            {
                case 1:
                    buttonText = Translation.GetString("doorLog");
                    buttonImage = HudManager.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton].Image;
                    break;
                default:
                    buttonText = Translation.GetString("security");
                    buttonImage = HudManager.Instance.UseButton.fastUseSettings[ImageNames.CamsButton].Image;
                    break;
            }

            this.CreateChargeAbilityButton(
                buttonText,
                buttonImage,
                CleanUp,
                checkAbility: IsClose);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {
            return this.IsCommonUse() && Minigame.Instance == null;
        }

        public bool IsClose() => Minigame.Instance != null;

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.monitoring != null)
            {
                this.monitoring.Close();
            }
            if (this.chargeTime != null)
            {
                this.chargeTime.gameObject.SetActive(false);
            }
        }

        public bool UseAbility()
        {
            if (this.monitoring == null)
            {
                // 0 = Skeld
                // 1 = Mira HQ
                // 2 = Polus
                // 3 = Dleks - deactivated
                // 4 = Airship
                SystemConsole watchConsole;

                switch (PlayerControl.GameOptions.MapId)
                {
                    case 0:
                        watchConsole = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(
                            x => x.gameObject.name.Contains("SurvConsole"));
                        break;
                    case 1:
                        watchConsole = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(
                            x => x.gameObject.name.Contains("SurvLogConsole"));
                        break;
                    case 2:
                        watchConsole = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(
                            x => x.gameObject.name.Contains("Surv_Panel"));
                        break;
                    case 4:
                        watchConsole = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(
                            x => x.gameObject.name.Contains("task_cams"));
                        break;
                    default:
                        return false;
                }

                if (watchConsole == null || Camera.main == null)
                {
                    return false;
                }
                this.monitoring = Object.Instantiate(
                    watchConsole.MinigamePrefab,
                    Camera.main.transform, false);
            }

            this.monitoring.transform.SetParent(Camera.main.transform, false);
            this.monitoring.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            this.monitoring.Begin(null);

            return true;
        }
        public void Update(PlayerControl rolePlayer)
        {
            if (this.chargeTime == null)
            {
                this.chargeTime = Object.Instantiate(
                    HudManager.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.chargeTime.transform.localPosition = new Vector3(3.5f, 2.25f, -250.0f);
            }

            if (!this.Button.IsAbilityActive())
            {
                this.chargeTime.gameObject.SetActive(false);
                return; 
            }

            this.chargeTime.gameObject.SetActive(true);
            this.chargeTime.text = Design.ColoedString(
                Palette.EnabledColor,
                Mathf.CeilToInt(this.Button.GetCurTime()).ToString());
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, true);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }
}
