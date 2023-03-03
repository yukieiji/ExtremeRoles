using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Watchdog : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public ExtremeAbilityButton Button
        {
            get => this.securityButton;
            set
            {
                this.securityButton = value;
            }
        }

        private ExtremeAbilityButton securityButton;
        private Minigame monitoring;
        private TMPro.TextMeshPro chargeTime;

        public Watchdog() : base(
            ExtremeRoleId.Watchdog,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Watchdog.ToString(),
            ColorPalette.WatchdogViolet,
            false, true, false, false)
        { }

        public void CleanUp()
        {
            if (this.monitoring != null)
            {
                this.monitoring.Close();
                this.monitoring = null;
            }
        }

        public void CreateAbility()
        {

            string buttonText;
            Sprite buttonImage;
            var imageDict = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;
            switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId))
            {
                case 1:
                    buttonText = "doorLog";
                    buttonImage = imageDict[ImageNames.DoorLogsButton].Image;
                    break;
                default:
                    buttonText = "securityCamera";
                    buttonImage = imageDict[ImageNames.CamsButton].Image;
                    break;
            }

            this.CreateChargeAbilityButton(
                buttonText,
                buttonImage,
                IsOpen,
                CleanUp);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {
            return this.IsCommonUse() && Minigame.Instance == null;
        }

        public bool IsOpen() => Minigame.Instance != null;

        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            if (this.monitoring != null)
            {
                this.monitoring.Close();
                this.monitoring = null;
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
                SystemConsole watchConsole = GameSystem.GetSecuritySystemConsole();
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
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.chargeTime.transform.localPosition = new Vector3(3.5f, 2.25f, -250.0f);
            }

            if (!this.Button.IsAbilityActive())
            {
                this.chargeTime.gameObject.SetActive(false);
                return; 
            }


            this.chargeTime.text = Mathf.CeilToInt(this.Button.Timer).ToString();
            this.chargeTime.gameObject.SetActive(true);
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, 3.0f);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }
}
