using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;

namespace ExtremeRoles.Roles.Combination;

public sealed class DetectiveOffice : ConstCombinationRoleManagerBase
{

    public const string Name = "DetectiveOffice";

    public DetectiveOffice() : base(
		CombinationRoleType.DetectiveOffice,
        Name, DefaultColor, 2,
        (GameSystem.VanillaMaxPlayerNum - 1) / 2)
    {
        this.Roles.Add(new Detective());
        this.Roles.Add(new Assistant());
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        base.CreateSpecificOption(factory);
		factory.IdOffset = this.Roles.Count * ExtremeRoleManager.OptionOffsetPerRole;
		factory.OptionPrefix = ExtremeRoleId.DetectiveApprentice.ToString();
		DetectiveApprentice.DetectiveApprenticeOptionHolder.CreateOption(factory);
    }

}

public sealed class Detective : MultiAssignRoleBase, IRoleMurderPlayerHook, IRoleResetMeeting, IRoleReportHook, IRoleUpdate, IRoleSpecialReset
{
    public readonly record struct CrimeInfo(
		Vector2 Pos,
		DateTime KilledTime,
		float ReportTime,
		PlayerStatus Reason,
		ExtremeRoleType KillerTeam,
		ExtremeRoleId KillerRole,
		RoleTypes KillerVanillaRole,
		byte Killer);

    public class CrimeInfoContainer
    {
        private Dictionary<byte, Vector2> deadBodyPos = new Dictionary<byte, Vector2>();
        private Dictionary<byte, float> timer = new Dictionary<byte, float>();

        public CrimeInfoContainer()
        {
            this.Clear();
        }

        public void Clear()
        {
            this.deadBodyPos.Clear();
            this.timer.Clear();
        }

        public void AddDeadBody(
            PlayerControl killerPlayer,
            PlayerControl deadPlayer)
        {
            this.deadBodyPos.Add(
                deadPlayer.PlayerId,
				deadPlayer.GetTruePosition());
            this.timer.Add(
                deadPlayer.PlayerId,
                0.0f);
        }

        public CrimeInfo? GetCrimeInfo(byte playerId)
        {
			if (!(
					this.deadBodyPos.TryGetValue(playerId, out var pos) &&
					ExtremeRolesPlugin.ShipState.DeadPlayerInfo.TryGetValue(playerId, out var state) &&
					state is not null &&
					state.Killer != null &&
					ExtremeRoleManager.TryGetRole(state.Killer.PlayerId, out var role)
				))
            {
                return null;
            }

			return new CrimeInfo(
				pos, state.DeadTime,
				this.timer[playerId],
				state.Reason,
				role.Team, role.Id,
				role.Id == ExtremeRoleId.VanillaRole ?
					((Solo.VanillaRoleWrapper)role).VanilaRoleId : RoleTypes.Crewmate
				state.Killer.PlayerId);
        }

        public void Update()
        {

            if (this.timer.Count == 0) { return; }

            foreach (byte playerId in this.timer.Keys)
            {
                this.timer[playerId] = this.timer[playerId] += Time.deltaTime;
            }
        }
    }

    public enum SearchCond : byte
    {
        None,
        FindKillTime,
        FindReportTime,
		FindReson,
        FindTeam,
        FindRole,
		FindName,
    }

    public enum DetectiveOption
    {
        SearchRange,
        SearchTime,
        SearchAssistantTime,
		SearchOnlyOnce,
		SearchCanFindName,
		SearchCanContine,
		TextShowTime,
    }

    private CrimeInfo? targetCrime;
    private CrimeInfoContainer info;
    private Arrow crimeArrow;
    private float searchTime;
    private float searchAssistantTime;
    private float timer = 0.0f;
    private float searchCrimeInfoTime;
    private float range;
    private SearchCond cond;
    private string searchStrBase;
    private TMPro.TextMeshPro searchText;
    private TextPopUpper textPopUp;
    private Vector2 prevPlayerPos;
    private static readonly Vector2 defaultPos = new Vector2(100.0f, 100.0f);
	private Dictionary<byte, SearchCond> condition = new Dictionary<byte, SearchCond>();

	private bool includeName;
	private bool onlyOnce;
	private bool canContine;

    public Detective() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Detective,
			ColorPalette.DetectiveKokikou),
        false, true, false, false,
        tab: OptionTab.CombinationTab)
    { }

    public void AllReset(PlayerControl rolePlayer)
    {
        upgradeAssistant();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        this.info.Clear();
    }

    public void ResetOnMeetingStart()
    {
        if (this.crimeArrow != null)
        {
            this.crimeArrow.SetActive(false);
        }
        resetSearchCond();
    }

    public void HookReportButton(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter)
    {
        this.targetCrime = null;
        this.searchCrimeInfoTime = float.MaxValue;
    }

    public void HookBodyReport(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter,
        NetworkedPlayerInfo reportBody)
    {
        this.targetCrime = this.info.GetCrimeInfo(reportBody.PlayerId);
        this.searchCrimeInfoTime = ExtremeRoleManager.TryGetSafeCastedRole<Assistant>(
			reporter.PlayerId, out var _)?
				this.searchAssistantTime : this.searchTime;
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {
        this.info.AddDeadBody(source, target);
    }

    public void Update(PlayerControl rolePlayer)
    {

        if (this.prevPlayerPos == defaultPos)
        {
            this.prevPlayerPos = rolePlayer.GetTruePosition();
        }
        if (this.info != null)
        {
            this.info.Update();
        }

        if (this.targetCrime != null)
        {
            if (this.crimeArrow == null)
            {
                this.crimeArrow = new Arrow(
                    ColorPalette.DetectiveKokikou);
            }

            var crime = this.targetCrime.Value;
            Vector2 crimePos = crime.Pos;

            this.crimeArrow.UpdateTarget(crimePos);
            this.crimeArrow.Update();
            this.crimeArrow.SetActive(true);

            Vector2 playerPos = rolePlayer.GetTruePosition();

            if (!PhysicsHelpers.AnythingBetween(
                    crimePos, playerPos,
                    Constants.ShipAndAllObjectsMask, false) &&
                Vector2.Distance(crimePos, playerPos) < this.range &&
                this.prevPlayerPos == rolePlayer.GetTruePosition())
            {

                updateSearchText();

                if (this.timer > 0.0f)
                {
                    this.timer -= Time.deltaTime;
                }
                else
                {
                    this.timer = this.searchCrimeInfoTime;
                    updateSearchCond(crime);
                }
            }
            else
            {
                this.timer = this.searchCrimeInfoTime;
                resetSearchCond();
            }
        }
        else
        {
            if (this.crimeArrow != null)
            {
                this.crimeArrow.SetActive(false);
            }
        }
        this.prevPlayerPos = rolePlayer.GetTruePosition();
    }
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        upgradeAssistant();
    }

    public override void ExiledAction(PlayerControl rolePlayer)
    {
        upgradeAssistant();
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            DetectiveOption.SearchRange,
            1.0f, 0.5f, 2.8f, 0.1f);

        factory.CreateFloatOption(
            DetectiveOption.SearchTime,
            6.0f, 3.0f, 10.0f, 0.1f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            DetectiveOption.SearchAssistantTime,
            4.0f, 2.0f, 7.5f, 0.1f,
            format: OptionUnit.Second);
		factory.CreateBoolOption(
			DetectiveOption.SearchOnlyOnce,
			true);
		factory.CreateBoolOption(
			DetectiveOption.SearchCanFindName,
			false);
		factory.CreateBoolOption(
			DetectiveOption.SearchCanContine,
			false);
		factory.CreateFloatOption(
            DetectiveOption.TextShowTime,
            60.0f, 5.0f, 120.0f, 0.1f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        this.cond = SearchCond.None;
        this.info = new CrimeInfoContainer();
        this.info.Clear();

        var loader = this.Loader;
        this.range = loader.GetValue<DetectiveOption, float>(
            DetectiveOption.SearchRange);
        this.searchTime = loader.GetValue<DetectiveOption, float>(
            DetectiveOption.SearchTime);
        this.searchAssistantTime = loader.GetValue<DetectiveOption, float>(
            DetectiveOption.SearchAssistantTime);

        this.textPopUp = new TextPopUpper(
            4, loader.GetValue<DetectiveOption, float>(DetectiveOption.TextShowTime),
            new Vector3(-3.75f, -2.5f, -250.0f),
            TMPro.TextAlignmentOptions.BottomLeft);
        this.searchCrimeInfoTime = float.MaxValue;
        this.prevPlayerPos = defaultPos;
    }

    private void updateSearchCond(CrimeInfo info)
    {
        this.cond++;
        showSearchResultText(info);
        if (this.cond == SearchCond.FindRole)
        {
            this.targetCrime = null;
            if (this.crimeArrow != null)
            {
                this.crimeArrow.SetActive(false);
            }
            if (this.searchText != null)
            {
                this.searchText.gameObject.SetActive(false);
            }
        }
    }
    private void resetSearchCond()
    {
        this.cond = SearchCond.None;
        if (this.searchText != null)
        {
            this.searchText.gameObject.SetActive(false);
        }
        if (this.textPopUp != null)
        {
            this.textPopUp.Clear();
        }
    }
    private void showSearchResultText(in CrimeInfo info)
    {
        if (this.textPopUp == null) { return; }

        string showStr = "";
		string key = this.cond.ToString();
		switch (this.cond)
        {
            case SearchCond.FindKillTime:
                showStr = Tr.GetString(
					key,
                    info.KilledTime.ToString());
                break;
            case SearchCond.FindReportTime:
                showStr = Tr.GetString(
					key, Mathf.CeilToInt(info.ReportTime));
                break;
			case SearchCond.FindReson:
				showStr = Tr.GetString(
					key,
					Tr.GetString(info.Reason.ToString()));
				break;
			case SearchCond.FindTeam:
                showStr = Tr.GetString(
					key, Tr.GetString(info.KillerTeam.ToString()));
                break;
            case SearchCond.FindRole:
                var role = info.KillerRole;
                string roleStr = Tr.GetString(info.KillerRole.ToString());
                if (role == ExtremeRoleId.VanillaRole)
                {
                    roleStr = Tr.GetString(info.KillerVanillaRole.ToString());
                }
                showStr = Tr.GetString(
					key, roleStr);
				if (!this.includeName)
				{
					this.cond++;
				}
                break;
			case SearchCond.FindName:
				var player = Player.GetPlayerControlById(info.Killer);
				if (player == null ||
					player.Data == null)
				{
					return;
				}
				showStr = Tr.GetString(key, player.Data.DefaultOutfit.PlayerName);
				break;
			default:
                break;
        }

        this.textPopUp.AddText(showStr);

    }

    private void updateSearchText()
    {
        if (this.searchText == null)
        {
            this.searchText = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                Camera.main.transform, false);
            this.searchText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
            this.searchText.enableWordWrapping = false;
            this.searchStrBase = Tr.GetString("searchStrBase");
        }

        this.searchText.gameObject.SetActive(true);
        this.searchText.text = string.Format(
            this.searchStrBase, Mathf.CeilToInt(this.timer));

    }
    private void upgradeAssistant()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null ||
				player.Data == null ||
				player.Data.IsDead ||
				player.Data.Disconnected ||
                !ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
				role.Core.Id is not ExtremeRoleId.Assistant ||
				!this.IsSameControlId(role))
            {
                continue;
            }
            DetectiveApprentice.ChangeToDetectiveApprentice(player.PlayerId);
            break;
        }
    }
}

public class Assistant : MultiAssignRoleBase, IRoleMurderPlayerHook, IRoleReportHook, IRoleSpecialReset
{
    private Dictionary<byte, DateTime> deadBodyInfo;
    public Assistant() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Assistant,
			ColorPalette.AssistantBluCapri),
        false, true, false, false,
        tab: OptionTab.CombinationTab)
    { }

    public void AllReset(PlayerControl rolePlayer)
    {
        downgradeDetective();
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {
		if (MeetingHud.Instance != null)
		{
			return;
		}

        this.deadBodyInfo.Add(target.PlayerId, DateTime.UtcNow);
    }

    public void HookReportButton(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter)
    {
        this.deadBodyInfo.Clear();
    }

    public void HookBodyReport(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter,
        NetworkedPlayerInfo reportBody)
    {
        if (ExtremeRoleManager.TryGetRole(rolePlayer.PlayerId, out var role) &&
			this.IsSameControlId(role) &&
			this.deadBodyInfo.TryGetValue(reportBody.PlayerId, out var date) &&
			AmongUsClient.Instance.AmClient &&
			HudManager.Instance != null)
        {
			HudManager.Instance.Chat.AddChat(
				PlayerControl.LocalPlayer,
				Tr.GetString(
					"reportedDeadBodyInfo",
					date.ToString()));
		}
        this.deadBodyInfo.Clear();
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        downgradeDetective();
    }

    public override void ExiledAction(PlayerControl rolePlayer)
    {
        downgradeDetective();
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    { }

    protected override void RoleSpecificInit()
    {
        this.deadBodyInfo = new Dictionary<byte, DateTime>();
    }
    private void downgradeDetective()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null ||
				player.Data == null ||
				player.Data.IsDead ||
				player.Data.Disconnected ||
                !ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
				role.Core.Id is not ExtremeRoleId.Detective ||
				!this.IsSameControlId(role))
            {
                continue;
            }
            DetectiveApprentice.ChangeToDetectiveApprentice(player.PlayerId);
            break;
        }
    }
}

public class DetectiveApprentice : MultiAssignRoleBase, IRoleAutoBuildAbility, IRoleReportHook
{

    public struct DetectiveApprenticeOptionHolder
    {
        public int OptionOffset;
        public bool HasOtherVision;
        public float Vision;
        public bool ApplyEnvironmentVisionEffect;
        public bool HasOtherButton;
        public int HasOtherButtonNum;

		public enum DetectiveApprenticeOption
		{
			HasOtherVision,
			Vision,
			ApplyEnvironmentVisionEffect,
			HasOtherButton,
			HasOtherButtonNum,
		}

        public static void CreateOption(
            AutoParentSetOptionCategoryFactory factory)
		{
			var visionOpt = factory.CreateBoolOption(
				DetectiveApprenticeOption.HasOtherVision,
				false);

			factory.CreateFloatOption(
				DetectiveApprenticeOption.Vision,
				2f, 0.25f, 5f, 0.25f,
				visionOpt,
				format: OptionUnit.Multiplier);

			factory.CreateBoolOption(
				DetectiveApprenticeOption.ApplyEnvironmentVisionEffect,
				false, visionOpt);

			IRoleAbility.CreateAbilityCountOption(
				factory, 1, 10, 3.0f);

			var buttonOpt = factory.CreateBoolOption(
				DetectiveApprenticeOption.HasOtherButton,
				false);
			factory.CreateIntOption(
				DetectiveApprenticeOption.HasOtherButtonNum,
				1, 1, 10, 1, buttonOpt,
				format: OptionUnit.Shot);
        }

        public static DetectiveApprenticeOptionHolder LoadOptions(in OptionLoadWrapper loader)
        {
            return new DetectiveApprenticeOptionHolder()
            {
                HasOtherVision = loader.GetValue<DetectiveApprenticeOption, bool>(
					DetectiveApprenticeOption.HasOtherVision),
                Vision = loader.GetValue<DetectiveApprenticeOption, float>(
					DetectiveApprenticeOption.Vision),
                ApplyEnvironmentVisionEffect = loader.GetValue<DetectiveApprenticeOption, bool>(
					DetectiveApprenticeOption.ApplyEnvironmentVisionEffect),
                HasOtherButton = loader.GetValue<DetectiveApprenticeOption, bool>(
					DetectiveApprenticeOption.HasOtherButton),
                HasOtherButtonNum = loader.GetValue<DetectiveApprenticeOption, int>(
					DetectiveApprenticeOption.HasOtherButtonNum),
            };
        }
    }

    public ExtremeAbilityButton Button
    {
        get => this.meetingButton;
        set
        {
            this.meetingButton = value;
        }
    }

    private bool useAbility;
    private bool hasOtherButton;
    private bool callAnotherButton;
    private int buttonNum;
    private ExtremeAbilityButton meetingButton;
    private Minigame meeting;

	public override IOptionLoader Loader { get; }

	public DetectiveApprentice(
		IOptionLoader loader,
        int gameControlId,
        DetectiveApprenticeOptionHolder option
        ) : base(
			RoleCore.BuildCrewmate(
				ExtremeRoleId.DetectiveApprentice,
				ColorPalette.DetectiveApprenticeKonai),
            false, true, false, false)
    {
		this.Loader = loader;
        this.SetControlId(gameControlId);
        this.HasOtherVision = option.HasOtherVision;
        if (this.HasOtherVision)
        {
            this.Vision = option.Vision;
            this.IsApplyEnvironmentVision = option.ApplyEnvironmentVisionEffect;
        }
        else
        {
            this.Vision = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.CrewLightMod);
        }
        this.hasOtherButton = option.HasOtherButton;
        this.buttonNum = option.HasOtherButtonNum;
        this.callAnotherButton = false;
    }

    public static void ChangeToDetectiveApprentice(
        byte playerId)
    {
        if (!ExtremeRoleManager.TryGetRole(playerId, out var prevRoleBase) || !(prevRoleBase is MultiAssignRoleBase prevRole))
        {
            return;
        }
        if (prevRole is IRoleResetMeeting detectiveReset)
        {
            detectiveReset.ResetOnMeetingStart();
        }

        bool hasAnotherRole = prevRole.AnotherRole != null;

        if (hasAnotherRole)
        {
            if (playerId == PlayerControl.LocalPlayer.PlayerId)
            {
                if (prevRole.AnotherRole is IRoleAbility abilityRole)
                {
                    abilityRole.Button.OnMeetingStart();
                    abilityRole.Button.OnMeetingEnd();
                }
                if (prevRole.AnotherRole is IRoleResetMeeting resetRole)
                {
                    resetRole.ResetOnMeetingStart();
                    resetRole.ResetOnMeetingEnd();
                }
            }
        }

		if (!OptionManager.Instance.TryGetCategory(
				OptionTab.CombinationTab,
				ExtremeRoleManager.GetCombRoleGroupId(CombinationRoleType.DetectiveOffice),
				out var cate))
		{
			return;
		}

		int offset = 2 * ExtremeRoleManager.OptionOffsetPerRole;
		var loader = new OptionLoadWrapper(cate, offset);
		DetectiveApprentice newRole = new DetectiveApprentice(
			loader,
            prevRole.GameControlId,
            DetectiveApprenticeOptionHolder.LoadOptions(loader));
        if (playerId == PlayerControl.LocalPlayer.PlayerId)
        {
            newRole.CreateAbility();
        }
        if (hasAnotherRole)
        {
            newRole.AnotherRole = null;
            newRole.CanHasAnotherRole = true;
            newRole.SetAnotherRole(prevRole.AnotherRole);
            newRole.Core.Team = prevRole.AnotherRole.Core.Team;
        }

        ExtremeRoleManager.SetNewRole(playerId, newRole);
    }

    public void CleanUp()
    {
        if (this.meeting != null)
        {
            this.meeting.Close();
            this.useAbility = false;
            this.meeting = null;
        }
    }

    public void CreateAbility()
    {

        this.CreateActivatingAbilityCountButton(
            "emergencyMeeting",
			UnityObjectLoader.LoadFromResources<Sprite>(ObjectPath.Meeting),
            abilityOff: CleanUp,
            checkAbility: IsOpen,
            isReduceOnActive: true);
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse() =>
        IRoleAbility.IsCommonUse() && Minigame.Instance == null;

    public bool IsOpen() => Minigame.Instance != null;

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        this.meeting = null;
        this.callAnotherButton = false;
    }

    public void ResetOnMeetingStart()
    {
        if (this.useAbility)
        {
            this.callAnotherButton = true;
        }
        CleanUp();
    }

    public bool UseAbility()
    {
        this.useAbility = false;
        SystemConsole emergencyConsole;
        if (CompatModManager.Instance.TryGetModMap(out var modMap))
        {
            emergencyConsole = modMap.GetSystemConsole(
                Compat.Interface.SystemConsoleType.EmergencyButton);
        }
        else
        {
            // 0 = Skeld
            // 1 = Mira HQ
            // 2 = Polus
            // 3 = Dleks - deactivated
            // 4 = Airship
			string key = Map.Id switch
			{
				0 or 1 or 3 => "EmergencyConsole",
				2 => "EmergencyButton",
				4 => "task_emergency",
				5 => "ConchEmergencyButton",
				_ => string.Empty,
			};
			var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
			emergencyConsole = systemConsoleArray.FirstOrDefault(x => x.gameObject.name.Contains(key));

		}

        if (emergencyConsole == null || Camera.main == null)
        {
            return false;
        }

        this.meeting = MinigameSystem.Open(
            emergencyConsole.MinigamePrefab);
        this.useAbility = true;

        return true;

    }

    public void HookReportButton(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter)
    {
        if (this.callAnotherButton &&
            PlayerControl.LocalPlayer.PlayerId == reporter.PlayerId &&
            this.hasOtherButton &&
            this.buttonNum > 0)
        {
            --this.buttonNum;
            ++rolePlayer.RemainingEmergencies;
            this.callAnotherButton = false;
        }
    }

    public void HookBodyReport(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter,
        NetworkedPlayerInfo reportBody)
    {
        return;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        throw new Exception("Don't call this class method!!");
    }

    protected override void RoleSpecificInit()
    {
        throw new Exception("Don't call this class method!!");
    }
}
