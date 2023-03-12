using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Supervisor : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum SuperviosrOption
        {
            IsBoostTask,
            TaskGage,
        }

        public ExtremeAbilityButton Button
        {
            get => this.adminButton;
            set
            {
                this.adminButton = value;
            }
        }

        public bool IsAbilityActive
        {
            get
            {
                if (this.adminButton == null) { return false; }

                return this.adminButton.IsAbilityActive();
            }
        }

        public bool Boosted;
        private bool isBoostTask;
        private float taskGage;

        private ExtremeAbilityButton adminButton;
        private TMPro.TextMeshPro chargeTime;

        public Supervisor() : base(
            ExtremeRoleId.Supervisor,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Supervisor.ToString(),
            ColorPalette.SupervisorLime,
            false, true, false, false)
        { }

        public void CleanUp()
        {
            MapBehaviour.Instance.Close();
        }

        public void CreateAbility()
        {
            this.CreateChargeAbilityButton(
                "admin",
                GameSystem.GetAdminButtonImage(),
                IsOpen, CleanUp);
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

        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            if (this.chargeTime != null)
            {
                this.chargeTime.gameObject.SetActive(false);
            }
        }

        public bool UseAbility()
        {
            FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(
                new MapOptions
                {
                    Mode = MapOptions.Modes.CountOverlay,
                    AllowMovementWhileMapOpen = true,
                    ShowLivePlayerPosition = true,
                    IncludeDeadBodies = true,
                });

            return true;
        }
        public void Update(PlayerControl rolePlayer)
        {

            if (this.isBoostTask && !this.Boosted)
            {
                if (Player.GetPlayerTaskGage(rolePlayer) >= this.taskGage)
                {
                    this.Boosted = true;
                }
            }

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

            var boostOption = this.CreateBoolOption(
                SuperviosrOption.IsBoostTask,
                false, parentOps);
            CreateIntOption(
                SuperviosrOption.TaskGage,
                100, 50, 100, 5,
                boostOption,
                format:OptionUnit.Percentage);
        }

        protected override void RoleSpecificInit()
        {
            this.isBoostTask = OptionHolder.AllOption[
                GetRoleOptionId(SuperviosrOption.IsBoostTask)].GetValue();
            this.taskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(SuperviosrOption.TaskGage)].GetValue() / 100.0f;
            this.RoleAbilityInit();
        }
    }
}
