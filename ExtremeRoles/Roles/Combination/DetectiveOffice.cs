using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;


namespace ExtremeRoles.Roles.Combination
{
    public class DetectiveOffice : ConstCombinationRoleManagerBase
    {

        public const string Name = "DetectiveOffice";

        public DetectiveOffice() : base(
            Name, new Color(255f, 255f, 255f), 2,
            (OptionHolder.VanillaMaxPlayerNum - 1) / 2)
        {
            this.Roles.Add(new Detective());
            this.Roles.Add(new Assistant());
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            base.CreateSpecificOption(parentOps);
            DetectiveApprentice.DetectiveApprenticeOptionHolder.CreateOption(
                parentOps, this.OptionIdOffset);
        }

    }

    public class Detective : MultiAssignRoleBase, IRoleMurderPlayerHock, IRoleResetMeeting, IRoleReportHock, IRoleUpdate, IRoleSpecialReset
    {
        public struct CrimeInfo
        {
            public Vector2 Position;
            public DateTime KilledTime;
            public float ReportTime;
            public ExtremeRoleType KillerTeam;
            public ExtremeRoleId KillerRole;
            public RoleTypes KillerVanillaRole;
        }

        public class CrimeInfoContainer
        {
            private Dictionary<byte, (Vector2, DateTime, byte)> deadBodyInfo = new Dictionary<byte, (Vector2, DateTime, byte)>();
            private Dictionary<byte, float> timer = new Dictionary<byte, float>();

            public CrimeInfoContainer()
            {
                this.Clear();
            }

            public void Clear()
            {
                this.deadBodyInfo.Clear();
                this.timer.Clear();
            }

            public void AddDeadBody(
                PlayerControl killerPlayer,
                PlayerControl deadPlayer)
            {
                this.deadBodyInfo.Add(
                    deadPlayer.PlayerId,
                    (
                        deadPlayer.GetTruePosition(),
                        DateTime.UtcNow,
                        killerPlayer.PlayerId
                    ));
                this.timer.Add(
                    deadPlayer.PlayerId,
                    0.0f);
            }

            public CrimeInfo? GetCrimeInfo(byte playerId)
            {
                if (!this.deadBodyInfo.ContainsKey(playerId))
                { 
                    return null;
                }

                var (pos, time, killerPlayerId) = this.deadBodyInfo[playerId];
                var role = ExtremeRoleManager.GameRole[killerPlayerId];

                return new CrimeInfo()
                {
                    Position = pos,
                    KilledTime = time,
                    ReportTime = this.timer[playerId],
                    KillerTeam = role.Team,
                    KillerRole = role.Id,
                    KillerVanillaRole = role.Id == ExtremeRoleId.VanillaRole ?
                        ((Solo.VanillaRoleWrapper)role).VanilaRoleId : RoleTypes.Crewmate
                };
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
            FindTeam,
            FindRole,
        }

        public enum DetectiveOption
        {
            SearchRange,
            SearchTime,
            SearchAssistantTime,
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

        public Detective() : base(
            ExtremeRoleId.Detective,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Detective.ToString(),
            ColorPalette.DetectiveKokikou,
            false, true, false, false)
        { }

        public void AllReset(PlayerControl rolePlayer)
        {
            upgradeAssistant();
        }

        public void ResetOnMeetingEnd()
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

        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter)
        {
            this.targetCrime = null;
            this.searchCrimeInfoTime = float.MaxValue;
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            this.targetCrime = this.info.GetCrimeInfo(reportBody.PlayerId);
            this.searchCrimeInfoTime = ExtremeRoleManager.GameRole[
                reporter.PlayerId].Id == ExtremeRoleId.Assistant ? this.searchAssistantTime : this.searchTime;
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            this.info.AddDeadBody(source, target);
        }

        public void Update(PlayerControl rolePlayer)
        {

            if (this.prevPlayerPos == null)
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

                var crime = (CrimeInfo)this.targetCrime;
                Vector2 crimePos = crime.Position;

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

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            upgradeAssistant();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CreateFloatOption(
                DetectiveOption.SearchRange,
                1.0f, 0.5f, 2.8f, 0.1f,
                parentOps);

            CreateFloatOption(
                DetectiveOption.SearchTime,
                6.0f, 3.0f, 10.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
                DetectiveOption.SearchAssistantTime,
                4.0f, 2.0f, 7.5f, 0.1f,
                parentOps, format: OptionUnit.Second);

            CreateFloatOption(
                DetectiveOption.TextShowTime,
                60.0f, 5.0f, 120.0f, 0.1f,
                parentOps, format: OptionUnit.Second);
        }

        protected override void RoleSpecificInit()
        {
            this.cond = SearchCond.None;
            this.info = new CrimeInfoContainer();
            this.info.Clear();

            var allOption = OptionHolder.AllOption;
            this.range = allOption[
                GetRoleOptionId(DetectiveOption.SearchRange)].GetValue();
            this.searchTime = allOption[
                GetRoleOptionId(DetectiveOption.SearchTime)].GetValue();
            this.searchAssistantTime = allOption[
                GetRoleOptionId(DetectiveOption.SearchAssistantTime)].GetValue();

            this.textPopUp = new TextPopUpper(
                4, allOption[GetRoleOptionId(DetectiveOption.TextShowTime)].GetValue(),
                new Vector3(-4.0f, -2.75f, -250.0f),
                TMPro.TextAlignmentOptions.BottomLeft);
            this.searchCrimeInfoTime = float.MaxValue;
        }

        private void updateSearchCond(CrimeInfo info)
        {
            this.cond += 1;
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
        private void showSearchResultText(CrimeInfo info)
        {
            if (this.textPopUp == null) { return; }
            
            string showStr = "";
            switch (this.cond)
            {
                case SearchCond.FindKillTime:
                    showStr = string.Format(
                        Translation.GetString(SearchCond.FindKillTime.ToString()),
                        info.KilledTime);
                    break;
                case SearchCond.FindReportTime:
                    showStr = string.Format(
                        Translation.GetString(SearchCond.FindReportTime.ToString()),
                        Mathf.CeilToInt(info.ReportTime));
                    break;
                case SearchCond.FindTeam:
                    showStr = string.Format(
                        Translation.GetString(SearchCond.FindTeam.ToString()),
                        Translation.GetString(info.KillerTeam.ToString()));
                    break;
                case SearchCond.FindRole:

                    var role = info.KillerRole;
                    string roleStr = Translation.GetString(info.KillerRole.ToString());

                    if (role == ExtremeRoleId.VanillaRole)
                    {
                        roleStr = Translation.GetString(info.KillerVanillaRole.ToString());
                    }

                    showStr = string.Format(
                        Translation.GetString(SearchCond.FindRole.ToString()),
                        roleStr);
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
                this.searchStrBase = Translation.GetString("searchStrBase");
            }

            this.searchText.gameObject.SetActive(true);
            this.searchText.text = string.Format(
                this.searchStrBase, Mathf.CeilToInt(this.timer));

        }
        private void upgradeAssistant()
        {
            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                if (role.Id != ExtremeRoleId.Assistant) { continue; }
                if (!this.IsSameControlId(role)) { continue; }

                var playerInfo = GameData.Instance.GetPlayerById(playerId);
                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    DetectiveApprentice.ChangeToDetectiveApprentice(playerId);
                    break;
                }
            }
        }
    }

    public class Assistant : MultiAssignRoleBase, IRoleMurderPlayerHock, IRoleReportHock, IRoleSpecialReset
    {
        private Dictionary<byte, DateTime> deadBodyInfo = new Dictionary<byte, DateTime>();
        public Assistant() : base(
            ExtremeRoleId.Assistant,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Assistant.ToString(),
            ColorPalette.AssistantBluCapri,
            false, true, false, false)
        { }

        public void AllReset(PlayerControl rolePlayer)
        {
            downgradeDetective();
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            this.deadBodyInfo.Add(target.PlayerId, DateTime.UtcNow);
        }

        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter)
        {
            this.deadBodyInfo.Clear();
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            if (this.IsSameControlId(ExtremeRoleManager.GameRole[rolePlayer.PlayerId]))
            {
                if (this.deadBodyInfo.ContainsKey(reportBody.PlayerId))
                {
                    if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                            PlayerControl.LocalPlayer,
                            string.Format(
                                Translation.GetString("reportedDeadBodyInfo"),
                                this.deadBodyInfo[reportBody.PlayerId]));
                    }
                }
            }
            this.deadBodyInfo.Clear();
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            downgradeDetective();
        }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            downgradeDetective();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        { }

        protected override void RoleSpecificInit()
        {
            this.deadBodyInfo.Clear();
        }
        private void downgradeDetective()
        {
            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                if (role.Id != ExtremeRoleId.Detective) { continue; }
                if (!this.IsSameControlId(role)) { continue; }
                
                var playerInfo = GameData.Instance.GetPlayerById(playerId);
                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    DetectiveApprentice.ChangeToDetectiveApprentice(playerId);
                    break;
                }
            }
        }
    }

    public class DetectiveApprentice : SingleRoleBase, IRoleAbility, IRoleReportHock
    {

        public struct DetectiveApprenticeOptionHolder
        {
            public int OptionOffset;
            public bool HasOtherVison;
            public float Vison;
            public bool ApplyEnvironmentVisionEffect;
            public bool HasOtherButton;
            public int HasOtherButtonNum;

            public enum DetectiveApprenticeOption
            {
                HasOtherVison,
                Vison,
                ApplyEnvironmentVisionEffect,
                HasOtherButton,
                HasOtherButtonNum,
            }

            public static void CreateOption(
                CustomOptionBase parentOps,
                int optionId)
            {
                int getRoleOptionId<T>(T option) where T : struct, IConvertible
                {
                    return optionId + Convert.ToInt32(option);
                }

                string roleName = ExtremeRoleId.DetectiveApprentice.ToString();

                var visonOption = new BoolCustomOption(
                    getRoleOptionId(DetectiveApprenticeOption.HasOtherVison),
                    string.Concat(
                        roleName,
                        DetectiveApprenticeOption.HasOtherVison.ToString()),
                    false, parentOps);

                new FloatCustomOption(
                    getRoleOptionId(DetectiveApprenticeOption.Vison),
                    string.Concat(
                        roleName,
                        DetectiveApprenticeOption.Vison.ToString()),
                    2f, 0.25f, 5f, 0.25f,
                    visonOption, format: OptionUnit.Multiplier);
                new BoolCustomOption(
                   getRoleOptionId(DetectiveApprenticeOption.ApplyEnvironmentVisionEffect),
                   string.Concat(
                       roleName,
                       DetectiveApprenticeOption.ApplyEnvironmentVisionEffect.ToString()),
                   false, visonOption);

                new IntCustomOption(
                    getRoleOptionId(RoleAbilityCommonOption.AbilityCount),
                    string.Concat(
                        roleName,
                        RoleAbilityCommonOption.AbilityCount.ToString()),
                    1, 1, 10, 1,
                    parentOps, format: OptionUnit.Shot);

                new FloatCustomOption(
                    getRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
                    string.Concat(
                        roleName,
                        RoleAbilityCommonOption.AbilityCoolTime.ToString()),
                    30.0f, 0.5f, 60f, 0.5f,
                    parentOps, format: OptionUnit.Second);

                new FloatCustomOption(
                    getRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
                    string.Concat(
                        roleName,
                        RoleAbilityCommonOption.AbilityActiveTime.ToString()),
                    3.0f, 1.0f, 5.0f, 0.5f,
                    parentOps, format: OptionUnit.Second);

                var buttonOption = new BoolCustomOption(
                    getRoleOptionId(DetectiveApprenticeOption.HasOtherButton),
                    string.Concat(
                        roleName,
                        DetectiveApprenticeOption.HasOtherButton.ToString()),
                    false, parentOps);
                new IntCustomOption(
                    getRoleOptionId(DetectiveApprenticeOption.HasOtherButtonNum),
                    string.Concat(
                        roleName,
                        DetectiveApprenticeOption.HasOtherButtonNum.ToString()),
                    1, 1, 10, 1, buttonOption,
                    format: OptionUnit.Shot);
            }

            public static DetectiveApprenticeOptionHolder LoadOptions(
                int optionId)
            {
                int getRoleOptionId(DetectiveApprenticeOption option)
                {
                    return optionId + (int)option;
                }

                var allOption = OptionHolder.AllOption;

                return new DetectiveApprenticeOptionHolder()
                {
                    OptionOffset = optionId,
                    HasOtherVison = allOption[
                        getRoleOptionId(DetectiveApprenticeOption.HasOtherVison)].GetValue(),
                    Vison = allOption[
                        getRoleOptionId(DetectiveApprenticeOption.Vison)].GetValue(),
                    ApplyEnvironmentVisionEffect = allOption[
                        getRoleOptionId(DetectiveApprenticeOption.ApplyEnvironmentVisionEffect)].GetValue(),
                    HasOtherButton = allOption[
                        getRoleOptionId(DetectiveApprenticeOption.HasOtherButton)].GetValue(),
                    HasOtherButtonNum = allOption[
                        getRoleOptionId(DetectiveApprenticeOption.HasOtherButtonNum)].GetValue(),
                };
            }

        }

        public RoleAbilityButtonBase Button
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
        private RoleAbilityButtonBase meetingButton;
        private Minigame meeting;

        public DetectiveApprentice(
            int gameControlId,
            DetectiveApprenticeOptionHolder option
            ) : base(
                ExtremeRoleId.DetectiveApprentice,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.DetectiveApprentice.ToString(),
                ColorPalette.DetectiveApprenticeKonai,
                false, true, false, false)
        {
            this.OptionIdOffset = option.OptionOffset;
            this.GameControlId = gameControlId;
            this.HasOtherVison = option.HasOtherVison;
            if (this.HasOtherVison)
            {
                this.Vison = option.Vison;
                this.IsApplyEnvironmentVision = option.ApplyEnvironmentVisionEffect;
            }
            else
            {
                this.Vison = PlayerControl.GameOptions.CrewLightMod;
            }
            this.hasOtherButton = option.HasOtherButton;
            this.buttonNum = option.HasOtherButtonNum;
            this.callAnotherButton = false;
        }

        public static void ChangeToDetectiveApprentice(
            byte playerId)
        {
            var prevRole = ExtremeRoleManager.GameRole[playerId] as MultiAssignRoleBase;
            if (prevRole == null) { return; }

            var detectiveReset = prevRole as IRoleResetMeeting;

            if (detectiveReset != null)
            {
                detectiveReset.ResetOnMeetingStart();
            }

            if (prevRole.AnotherRole != null)
            {
                if (playerId == PlayerControl.LocalPlayer.PlayerId)
                {

                    var abilityRole = prevRole.AnotherRole as IRoleAbility;
                    if (abilityRole != null)
                    {
                        abilityRole.ResetOnMeetingStart();
                    }
                    var resetRole = prevRole.AnotherRole as IRoleResetMeeting;
                    if (resetRole != null)
                    {
                        resetRole.ResetOnMeetingStart();
                    }
                }
                var targetPlayer = Player.GetPlayerControlById(playerId);
                if (targetPlayer != null)
                {
                    var specialResetRole = prevRole.AnotherRole as IRoleSpecialReset;
                    if (specialResetRole != null)
                    {
                        specialResetRole.AllReset(targetPlayer);
                    }
                }
            }

            DetectiveApprentice newRole = new DetectiveApprentice(
                prevRole.GameControlId,
                DetectiveApprenticeOptionHolder.LoadOptions(
                    prevRole.GetManagerOptionId(0)));
            if (playerId == PlayerControl.LocalPlayer.PlayerId)
            {
                newRole.CreateAbility();
            }

            ExtremeRoleManager.GameRole[playerId] = newRole;
        }

        public void CleanUp()
        {
            if (this.meeting != null)
            {
                this.meeting.Close();
                this.useAbility = false;
            }
        }

        public void CreateAbility()
        {

            this.CreateAbilityCountButton(
                Translation.GetString("emergencyMeeting"),
                Loader.CreateSpriteFromResources(
                    Path.DetectiveApprenticeEmergencyMeeting),
                abilityCleanUp: CleanUp,
                checkAbility: IsOpen);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {
            return this.IsCommonUse() && Minigame.Instance == null;
        }

        public bool IsOpen() => Minigame.Instance != null;

        public void RoleAbilityResetOnMeetingEnd()
        {
            this.callAnotherButton = false;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.useAbility)
            {
                this.callAnotherButton = true;
            }
            CleanUp();
        }

        public bool UseAbility()
        {

            if (this.meeting == null)
            {
                // 0 = Skeld
                // 1 = Mira HQ
                // 2 = Polus
                // 3 = Dleks - deactivated
                // 4 = Airship
                SystemConsole emergencyConsole;
                var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
                switch (PlayerControl.GameOptions.MapId)
                {
                    case 0:
                    case 1:
                    case 3:
                        emergencyConsole = systemConsoleArray.FirstOrDefault(
                            x => x.gameObject.name.Contains("EmergencyConsole"));
                        break;
                    case 2:
                        emergencyConsole = systemConsoleArray.FirstOrDefault(
                            x => x.gameObject.name.Contains("EmergencyButton"));
                        break;
                    case 4:
                        emergencyConsole = systemConsoleArray.FirstOrDefault(
                            x => x.gameObject.name.Contains("task_emergency"));
                        break;
                    default:
                        return false;
                }
                
                if (emergencyConsole == null || Camera.main == null)
                {
                    return false;
                }
                
                this.meeting = UnityEngine.Object.Instantiate(
                    emergencyConsole.MinigamePrefab,
                    Camera.main.transform, false);
            }

            this.meeting.transform.SetParent(Camera.main.transform, false);
            this.meeting.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            this.meeting.Begin(null);
            this.useAbility = true;

            return true;

        }

        public void HockReportButton(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter)
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

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new Exception("Don't call this class method!!");
        }

        protected override void RoleSpecificInit()
        {
            throw new Exception("Don't call this class method!!");
        }
    }
}
