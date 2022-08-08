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
    public sealed class TraitorManager : FlexibleCombinationRoleManagerBase
    {
        public TraitorManager() : base(new Traitor(), 1, false)
        { }

        public override void AssignSetUpInit(int curImpNum)
        {
            foreach (var role in this.Roles)
            {
                role.CanHasAnotherRole = true;
            }
        }

        public override MultiAssignRoleBase GetRole(
            int roleId, RoleTypes playerRoleType)
        {

            MultiAssignRoleBase role = null;

            if (this.BaseRole.Id != (ExtremeRoleId)roleId) { return role; }

            this.BaseRole.CanHasAnotherRole = true;

            return (MultiAssignRoleBase)this.BaseRole.Clone();
        }

        protected override void CommonInit()
        {
            this.Roles.Clear();
            int roleAssignNum = 1;
            var allOptions = OptionHolder.AllOption;

            this.BaseRole.CanHasAnotherRole = true;

            // 0:オフ、1:オン
            OptionHolder.ExecuteWithBlockOptionShare(
                () =>
                { 
                    allOptions[GetRoleOptionId(CombinationRoleCommonOption.IsMultiAssign)].UpdateSelection(1);
                });

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

    public sealed class Traitor : MultiAssignRoleBase, IRoleAbility, IRoleUpdate, IRoleSpecialSetUp
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
            {  }
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
        private string crewRoleStr;

        private AbilityType curAbilityType;
        private AbilityType nextUseAbilityType;
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
                ColorPalette.TraitorLightShikon,
                true, false, true, false,
                tab: OptionTab.Combination)
        {
            this.CanHasAnotherRole = true;
        }

        public void CreateAbility()
        {

            this.adminSprite = getAdminButtonImage();
            this.securitySprite = getSecurityImage();
            this.vitalSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
                ImageNames.VitalsButton].Image;

            this.Button = new TraitorCrackButton(
                Translation.GetString("traitorCracking"),
                UseAbility,
                IsAbilityUse,
                this.adminSprite,
                new Vector3(-1.8f, -0.06f, 0),
                CleanUp,
                CheckAbility,
                KeyCode.F,
                false);
            
            this.RoleAbilityInit();
        }

        public bool UseAbility()
        {
            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    FastDestroyableSingleton<HudManager>.Instance.ShowMap(
                        (System.Action<MapBehaviour>)(m => m.ShowCountOverlay()));
                    break;
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
                    openConsole(watchConsole.MinigamePrefab);
                    break;
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
                    openConsole(vitalConsole.MinigamePrefab);
                    break;
                default:
                    return false;
            }

            this.curAbilityType = this.nextUseAbilityType;

            updateAbility();
            updateButtonSprite();

            return true;
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
                    if (MapBehaviour.Instance)
                    {
                        MapBehaviour.Instance.Close();
                    }
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
        }

        public bool IsAbilityUse()
        {
         
            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    return
                        this.IsCommonUse() &&
                        (
                            MapBehaviour.Instance == null ||
                            !MapBehaviour.Instance.isActiveAndEnabled
                        );
                case AbilityType.Security:
                case AbilityType.Vital:
                    return this.IsCommonUse() && Minigame.Instance == null;
                default:
                    return false;
            }
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            this.Button.PositionOffset = new Vector3(-1.8f, -0.06f, 0);
            this.Button.ReplaceHotKey(KeyCode.F);

            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.ReplaceRole,
                new System.Collections.Generic.List<byte>
                {
                    playerId,
                    playerId,
                    (byte)ExtremeRoleManager.ReplaceOperation.ResetVanillaRole
                });
            RPCOperator.ReplaceRole(
                playerId, playerId,
                (byte)ExtremeRoleManager.ReplaceOperation.ResetVanillaRole);
        }

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

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
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
            this.crewRoleStr = this.AnotherRole.Id.ToString();
            if (this.AnotherRole.Id == ExtremeRoleId.VanillaRole)
            {
                this.crewRoleStr = this.AnotherRole.RoleName;
            }
            Logging.Debug($"Traitor Get Role:{this.crewRoleStr}");
            
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
                resetRole.AllReset(Player.GetPlayerControlById(rolePlayerId));
            }
            this.AnotherRole = null;
        }

        public override string GetIntroDescription()
        {
            return string.Format(
                base.GetIntroDescription(),
                Translation.GetString(this.crewRoleStr));
        }

        public override string GetFullDescription()
        {
            return string.Format(
                base.GetFullDescription(),
                Translation.GetString(this.crewRoleStr));
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, 5.0f);
        }

        protected override void RoleSpecificInit()
        {
            this.canUseButton = false;
            this.nextUseAbilityType = AbilityType.Admin;
            this.RoleAbilityInit();
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
                case 1:
                case 3:
                    return null;
                case 2:
                case 4:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("panel_vitals"));
                default:
                    return null;
            }
        }

        private void openConsole(Minigame game)
        {
            this.minigame = Object.Instantiate(game, Camera.main.transform, false);
            this.minigame.transform.SetParent(Camera.main.transform, false);
            this.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            this.minigame.Begin(null);
        }

        private void updateAbility()
        {
            ++this.nextUseAbilityType;
            this.nextUseAbilityType = (AbilityType)((int)this.nextUseAbilityType % 3);
            if (this.nextUseAbilityType == AbilityType.Vital &&
                (
                    PlayerControl.GameOptions.MapId == 0 ||
                    PlayerControl.GameOptions.MapId == 1 ||
                    PlayerControl.GameOptions.MapId == 3
                ))
            {
                this.nextUseAbilityType = AbilityType.Admin;
            }
        }
        private void updateButtonSprite()
        {
            var traitorButton = this.Button as TraitorCrackButton;

            Sprite sprite = Resources.Loader.CreateSpriteFromResources(
                Resources.Path.TestButton);

            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    sprite = this.adminSprite;
                    break;
                case AbilityType.Security:
                    sprite = this.securitySprite;
                    break;
                case AbilityType.Vital:
                    sprite = this.vitalSprite;
                    break;
                default:
                    break;
            }
            traitorButton.SetSprite(sprite);
        }
    }
}
