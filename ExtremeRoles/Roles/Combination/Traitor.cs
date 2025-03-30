using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;





using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination;

public sealed class TraitorManager : FlexibleCombinationRoleManagerBase
{
    public TraitorManager() : base(
		CombinationRoleType.Traitor,
		new Traitor(), 1, false)
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
        var loader = this.Loader;

        this.BaseRole.CanHasAnotherRole = true;

		// 0:オフ、1:オン
		if (loader.TryGetValueOption<CombinationRoleCommonOption, bool>(
				CombinationRoleCommonOption.IsMultiAssign, out var option))
		{
			option.Selection = 1;
		}

		if (loader.TryGetValueOption<CombinationRoleCommonOption, int>(
				CombinationRoleCommonOption.AssignsNum, out var o))
        {
            roleAssignNum = o.Value;
        }

        for (int i = 0; i < roleAssignNum; ++i)
        {
            this.Roles.Add((MultiAssignRoleBase)this.BaseRole.Clone());
        }
    }

}

public sealed class Traitor : MultiAssignRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleSpecialSetUp
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
            tab: OptionTab.CombinationTab)
    {
        this.CanHasAnotherRole = true;
    }

    public void CreateAbility()
    {

        this.adminSprite = GameSystem.GetAdminButtonImage();
        this.securitySprite = GameSystem.GetSecurityImage();
        this.vitalSprite = GameSystem.GetVitalImage();

        this.CreateBatteryAbilityButton(
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
                HudManager.Instance.ToggleMapVisible(
                    new MapOptions
                    {
                        Mode = MapOptions.Modes.CountOverlay,
                        AllowMovementWhileMapOpen = true,
                        ShowLivePlayerPosition = true,
                        IncludeDeadBodies = true,
                    });
                break;
            case AbilityType.Security:
                SystemConsole watchConsole = Map.GetSecuritySystemConsole();
                if (watchConsole == null || Camera.main == null)
                {
                    return false;
                }
                this.minigame = MinigameSystem.Open(
                    watchConsole.MinigamePrefab);
                break;
            case AbilityType.Vital:
				VitalsMinigame vital = MinigameSystem.Vital;
				if (vital == null || Camera.main == null)
                {
                    return false;
                }
                this.minigame = MinigameSystem.Open(vital);
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
                    IRoleAbility.IsCommonUse() &&
                    (
                        MapBehaviour.Instance == null ||
                        !MapBehaviour.Instance.isActiveAndEnabled
                    );
            case AbilityType.Security:
            case AbilityType.Vital:
                return IRoleAbility.IsCommonUse() && Minigame.Instance == null;
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
        this.Button.HotKey = KeyCode.F;

        byte playerId = PlayerControl.LocalPlayer.PlayerId;

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

    public void ResetOnMeetingStart()
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

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
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
                HudManager.Instance.KillButton.cooldownTimerText,
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

        if (PlayerControl.LocalPlayer.PlayerId == rolePlayerId)
        {
            if (this.AnotherRole is IRoleAbility abilityRole &&
				abilityRole.Button is not null)
            {
                abilityRole.Button.OnMeetingStart();
            }
            if (this.AnotherRole is IRoleResetMeeting meetingResetRole)
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
            Tr.GetString(this.crewRoleStr));
    }

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            Tr.GetString(this.crewRoleStr));
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory, 5.0f);
    }

    protected override void RoleSpecificInit()
    {
        this.canUseButton = false;
        this.nextUseAbilityType = AbilityType.Admin;
    }

    private void updateAbility()
    {
        ++this.nextUseAbilityType;
        this.nextUseAbilityType = (AbilityType)((int)this.nextUseAbilityType % 3);
    }
    private void updateButtonSprite()
    {
        if (this.Button.Behavior is not BatteryBehavior chargableAbility)
        {
            return;
        }

        Sprite sprite = Resources.UnityObjectLoader.LoadSpriteFromResources(
            Resources.ObjectPath.TestButton);

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
