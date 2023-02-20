using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;

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

        public ExtremeAbilityButton Button
        { 
            get => this.crackButton;
            set
            {
                this.crackButton = value;
            }
        }

        private ExtremeAbilityButton crackButton;
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

            this.adminSprite = GameSystem.GetAdminButtonImage();
            this.securitySprite = GameSystem.GetSecurityImage();
            this.vitalSprite = GameSystem.GetVitalImage();

            this.CreateChargeAbilityButton(
                "traitorCracking",
                this.adminSprite,
                checkAbility: CheckAbility,
                abilityOff: CleanUp);
        }

        public bool UseAbility()
        {
            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(
                        new MapOptions
                        {
                            Mode = MapOptions.Modes.CountOverlay,
                            AllowMovementWhileMapOpen = true,
                            ShowLivePlayerPosition = true,
                            IncludeDeadBodies = true,
                        });
                    break;
                case AbilityType.Security:
                    SystemConsole watchConsole = GameSystem.GetSecuritySystemConsole();
                    if (watchConsole == null || Camera.main == null)
                    {
                        return false;
                    }
                    this.minigame = GameSystem.OpenMinigame(
                        watchConsole.MinigamePrefab);
                    break;
                case AbilityType.Vital:
                    SystemConsole vitalConsole = GameSystem.GetVitalSystemConsole();
                    if (vitalConsole == null || Camera.main == null)
                    {
                        return false;
                    }
                    this.minigame = GameSystem.OpenMinigame(
                        vitalConsole.MinigamePrefab);
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
                        this.minigame = null;
                    }
                    break;
                default:
                    break;
            }
        }

        public bool IsAbilityUse()
        {
            if (!this.canUseButton) { return false; }
         
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
            this.Button.SetHotKey(KeyCode.F);

            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.ReplaceRole))
            {
                caller.WriteByte(playerId);
                caller.WriteByte(playerId);
                caller.WriteByte((byte)ExtremeRoleManager.ReplaceOperation.ResetVanillaRole);
            }
            RPCOperator.ReplaceRole(
                playerId, playerId,
                (byte)ExtremeRoleManager.ReplaceOperation.ResetVanillaRole);
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.chargeTime != null)
            {
                this.chargeTime.gameObject.SetActive(false);
            }
            if (this.minigame != null)
            {
                this.minigame.Close();
                this.minigame = null;
            }
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.canUseButton && this.Button != null)
            {
                this.Button.SetButtonShow(false);
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

        public override bool IsSameTeam(SingleRoleBase targetRole) =>
            this.IsNeutralSameTeam(targetRole);

        public override bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            this.canUseButton = true;
            if (this.Button != null)
            {
                this.Button.SetButtonShow(true);
            }
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

        private void updateAbility()
        {
            ++this.nextUseAbilityType;
            this.nextUseAbilityType = (AbilityType)((int)this.nextUseAbilityType % 3);

            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId);

            if (this.nextUseAbilityType == AbilityType.Vital &&
                (
                    mapId == 0 ||
                    mapId == 1 ||
                    mapId == 3
                ))
            {
                this.nextUseAbilityType = AbilityType.Admin;
            }
        }
        private void updateButtonSprite()
        {
            if (this.Button.Behavior is not ChargableAbilityBehavior chargableAbility)
            {
                return;
            }

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
            chargableAbility.SetButtonImage(sprite);
        }
    }
}
