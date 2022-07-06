using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.AbilityButton.Roles;

namespace ExtremeRoles.Roles.Combination
{
    public class TraitorManager : FlexibleCombinationRoleManagerBase
    {
        public TraitorManager() : base(new Traitor(), 1, false)
        { }

        protected override void CommonInit()
        {
            this.Roles.Clear();
            int roleAssignNum = 1;
            var allOptions = OptionHolder.AllOption;

            this.BaseRole.CanHasAnotherRole = true;

            if (allOptions.ContainsKey(GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)))
            {
                roleAssignNum = allOptions[
                    GetRoleOptionId(CombinationRoleCommonOption.AssignsNum)].GetValue();
            }

            for (int i = 0; i < roleAssignNum; ++i)
            {
                this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
            }
        }

    }

    public class Traitor : MultiAssignRoleBase, IRoleAbility, IRoleUpdate
    {
        public class TraitorCrackButton : ChargableButton
        {
            public TraitorCrackButton(
                string buttonText,
                System.Func<bool> ability,
                System.Func<bool> canUse,
                Sprite sprite,
                Vector3 positionOffset,
                System.Action abilityCleanUp,
                System.Func<bool> abilityCheck = null,
                KeyCode hotkey = KeyCode.F,
                bool mirror = false) : base(
                    buttonText, ability,
                    canUse, sprite,
                    positionOffset,
                    abilityCleanUp,
                    abilityCheck,
                    hotkey, mirror)
            {
            }
            public void SetSprite(Sprite img)
            {
                this.ButtonSprite = img;
            }
        }

        public enum AbilityType : byte
        {
            Admin,
            Security,
            Vital,
        }

        private bool canUseButton = false;
        private ExtremeRoleId crewRole;

        private AbilityType curAbilityType;
        private TMPro.TextMeshPro chargeTime;

        private Sprite adminSprite;
        private Sprite securitySprite;
        private Sprite vitalSprite;

        public RoleAbilityButtonBase Button
        { 
            get => this.crackButton;
            set
            {
                this.crackButton = value;
            }
        }

        private RoleAbilityButtonBase crackButton;
        private Minigame minigame;

        public Traitor(
            ) : base(
                ExtremeRoleId.Traitor,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Traitor.ToString(),
                ColorPalette.TraitorShikon,
                true, false, false, false)
        { }

        public void CreateAbility()
        {

            this.Button = new TraitorCrackButton(
                Translation.GetString("traitorCracking"),
                UseAbility,
                IsAbilityUse,
                getAdminButtonImage(),
                new Vector3(-1.8f, -0.06f, 0),
                CleanUp,
                CheckAbility,
                KeyCode.F,
                false);

            this.RoleAbilityInit();
        }

        public bool UseAbility()
        {
            switch (this.curAbilityType)
            {
                case AbilityType.Admin:
                    FastDestroyableSingleton<HudManager>.Instance.ShowMap(
                        (System.Action<MapBehaviour>)(m => m.ShowCountOverlay()));
                    return true;
                case AbilityType.Security:
                    SystemConsole watchConsole;
                    if (ExtremeRolesPlugin.Compat.IsModMap)
                    {
                        watchConsole = ExtremeRolesPlugin.Compat.ModMap.GetSystemConsole(
                            Compat.Interface.SystemConsoleType.SecurityCamera);
                    }
                    else
                    {
                        watchConsole = getSecurityConsole();
                    }

                    if (watchConsole == null || Camera.main == null)
                    {
                        return false;
                    }
                    this.minigame = Object.Instantiate(
                        watchConsole.MinigamePrefab,
                        Camera.main.transform, false);
                    this.minigame.transform.SetParent(Camera.main.transform, false);
                    this.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    this.minigame.Begin(null);
                    return true;
                case AbilityType.Vital:
                    SystemConsole vitalConsole;
                    if (ExtremeRolesPlugin.Compat.IsModMap)
                    {
                        vitalConsole = ExtremeRolesPlugin.Compat.ModMap.GetSystemConsole(
                            Compat.Interface.SystemConsoleType.Vital);
                    }
                    else
                    {
                        vitalConsole = getVitalConsole();
                    }

                    if (vitalConsole == null || Camera.main == null)
                    {
                        return false;
                    }
                    this.minigame = Object.Instantiate(
                        vitalConsole.MinigamePrefab,
                        Camera.main.transform, false);
                    this.minigame.transform.SetParent(Camera.main.transform, false);
                    this.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    this.minigame.Begin(null);
                    return true;
                default:
                    return false;
            }
        }

        public bool CheckAbility()
        {
            switch (this.curAbilityType)
            {
                case AbilityType.Admin:
                    return MapBehaviour.Instance.isActiveAndEnabled;
                case AbilityType.Security:
                case AbilityType.Vital:
                    return Minigame.Instance != null;
                default:
                    return false;
            }
        }

        public void CleanUp()
        {
            switch (this.curAbilityType)
            {
                case AbilityType.Admin:
                    MapBehaviour.Instance.Close();
                    break;
                case AbilityType.Security:
                case AbilityType.Vital:
                    if (this.minigame != null)
                    {
                        this.minigame.Close();
                    }
                    break;
                default:
                    break;
            }

            ++this.curAbilityType;
            this.curAbilityType = (AbilityType)((int)this.curAbilityType % 3);

            var traitorButton = this.Button as TraitorCrackButton;

            Sprite sprite = Resources.Loader.CreateSpriteFromResources(
                Resources.Path.TestButton);

            switch (this.curAbilityType)
            {
                case AbilityType.Admin:
                    sprite = getAdminButtonImage();
                    break;
                case AbilityType.Security:
                    sprite = getSecurityImage();
                    break;
                case AbilityType.Vital:
                    sprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                        ImageNames.VitalsButton].Image;
                    break;
                default:
                    break;
            }
            traitorButton.SetSprite(sprite);
        }

        public bool IsAbilityUse() => this.IsCommonUse();


        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            if (this.chargeTime != null)
            {
                this.chargeTime.gameObject.SetActive(false);
            }
            CleanUp();
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.canUseButton && this.Button != null)
            {
                this.Button.SetActive(false);
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

            this.chargeTime.text = Mathf.CeilToInt(this.Button.GetCurTime()).ToString();
            this.chargeTime.gameObject.SetActive(true);
        }

        public override bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            this.canUseButton = true;
            return true;
        }

        public override void OverrideAnotherRoleSetting()
        {
            this.CanHasAnotherRole = false;

            this.Team = ExtremeRoleType.Neutral;
            this.crewRole = this.AnotherRole.Id;
            
            byte rolePlayerId = byte.MaxValue;

            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                if (this.GameControlId == role.GameControlId)
                {
                    rolePlayerId = playerId;
                    break;
                }
            }
            if (rolePlayerId == byte.MaxValue) { return; }

            if (CachedPlayerControl.LocalPlayer.PlayerId == rolePlayerId)
            {
                var abilityRole = this.AnotherRole as IRoleAbility;
                if (abilityRole != null)
                {
                    abilityRole.ResetOnMeetingStart();
                }
                var meetingResetRole = this.AnotherRole as IRoleResetMeeting;
                if (meetingResetRole != null)
                {
                    meetingResetRole.ResetOnMeetingStart();
                }
            }

            var resetRole = this.AnotherRole as IRoleSpecialReset;
            if (resetRole != null)
            {
                resetRole.AllReset(
                    Player.GetPlayerControlById(rolePlayerId));
            }
        }

        public override string GetIntroDescription()
        {
            return string.Format(
                base.GetIntroDescription(),
                Translation.GetString(this.crewRole.ToString()));
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
        }

        protected override void RoleSpecificInit()
        {
            this.canUseButton = false;
            this.curAbilityType = AbilityType.Admin;
        }

        private Sprite getAdminButtonImage()
        {
            var imageDict = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;
            switch (PlayerControl.GameOptions.MapId)
            {
                case 0:
                case 3:
                    return imageDict[ImageNames.AdminMapButton].Image;
                case 1:
                    return imageDict[ImageNames.MIRAAdminButton].Image;
                case 2:
                    return imageDict[ImageNames.PolusAdminButton].Image;
                default:
                    return imageDict[ImageNames.AirshipAdminButton].Image;
            }
        }
        private Sprite getSecurityImage()
        {
            var imageDict = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;
            switch (PlayerControl.GameOptions.MapId)
            {
                case 1:
                    return imageDict[ImageNames.DoorLogsButton].Image;
                default:
                    return imageDict[ImageNames.CamsButton].Image;
            }
        }
        private SystemConsole getSecurityConsole()
        {
            // 0 = Skeld
            // 1 = Mira HQ
            // 2 = Polus
            // 3 = Dleks - deactivated
            // 4 = Airship
            var systemConsoleArray = Object.FindObjectsOfType<SystemConsole>();
            switch (PlayerControl.GameOptions.MapId)
            {
                case 0:
                case 3:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("SurvConsole"));
                case 1:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("SurvLogConsole"));
                case 2:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("Surv_Panel"));
                case 4:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("task_cams"));
                default:
                    return null;
            }
        }
        private SystemConsole getVitalConsole()
        {
            // 0 = Skeld
            // 1 = Mira HQ
            // 2 = Polus
            // 3 = Dleks - deactivated
            // 4 = Airship
            var systemConsoleArray = Object.FindObjectsOfType<SystemConsole>();
            switch (PlayerControl.GameOptions.MapId)
            {
                case 0:
                case 3:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("SurvConsole"));
                case 1:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("SurvLogConsole"));
                case 2:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("Surv_Panel"));
                case 4:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("task_cams"));
                default:
                    return null;
            }
        }
    }
}
