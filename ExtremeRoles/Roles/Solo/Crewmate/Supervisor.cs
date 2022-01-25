using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Supervisor : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public Supervisor() : base(
            ExtremeRoleId.Supervisor,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Supervisor.ToString(),
            ColorPalette.SupervisorLime,
            false, true, false, false)
        { }

        public RoleAbilityButtonBase Button
        { 
            get => this.adminButton;
            set
            {
                this.adminButton = value;
            }
        }

        private RoleAbilityButtonBase adminButton;
        private TMPro.TextMeshPro chargeTime;

        public void CleanUp()
        {
            MapBehaviour.Instance.Close();
        }

        public void CreateAbility()
        {
            Sprite buttonImage;
            var imageDict = HudManager.Instance.UseButton.fastUseSettings;
            switch (PlayerControl.GameOptions.MapId)
            {
                case 0:
                case 3:
                    buttonImage = imageDict[ImageNames.AdminMapButton].Image;
                    break;
                case 1:
                    buttonImage = imageDict[ImageNames.MIRAAdminButton].Image;
                    break;
                case 2:
                    buttonImage = imageDict[ImageNames.PolusAdminButton].Image;
                    break;
                case 4:
                    buttonImage = imageDict[ImageNames.PolusAdminButton].Image;
                    break;
                default:
                    buttonImage = Loader.CreateSpriteFromResources(
                        Path.TestButton, 115f);
                    break;
            }

            this.CreateChargeAbilityButton(
                Translation.GetString("admin"),
                buttonImage,
                CleanUp,
                checkAbility: IsOpen);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {
            return 
                this.IsCommonUse() && (
                    MapBehaviour.Instance == null || 
                    !MapBehaviour.Instance.isActiveAndEnabled);
        }

        public bool IsOpen() => MapBehaviour.Instance.isActiveAndEnabled;

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.chargeTime != null)
            {
                this.chargeTime.gameObject.SetActive(false);
            }
        }

        public bool UseAbility()
        {
            DestroyableSingleton<HudManager>.Instance.ShowMap(
                (System.Action<MapBehaviour>)(m => m.ShowCountOverlay()));

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
