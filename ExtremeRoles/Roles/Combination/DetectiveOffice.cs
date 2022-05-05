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
    }

    public class Detective : MultiAssignRoleBase, IRoleMurderPlayerHock, IRoleResetMeeting, IRoleReportHock, IRoleUpdate
    {
        public struct CrimeInfo
        {
            public Vector2 Position;
            public DateTime KilledTime;
            public float ReportTime;
            public ExtremeRoleType KillerTeam;
            public ExtremeRoleId KillerRole;
        }

        public class CrimeInfoContainer
        {
            private Dictionary<byte, (Vector2, DateTime)> deadBodyInfo = new Dictionary<byte, (Vector2, DateTime)>();
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
                PlayerControl deadPlayer)
            {
                this.deadBodyInfo.Add(
                    deadPlayer.PlayerId,
                    (
                        deadPlayer.GetTruePosition(),
                        DateTime.UtcNow
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

                var (pos, time) = this.deadBodyInfo[playerId];
                var role = ExtremeRoleManager.GameRole[playerId];

                return new CrimeInfo()
                {
                    Position = pos,
                    KilledTime = time,
                    ReportTime = this.timer[playerId],
                    KillerTeam = role.Team,
                    KillerRole = role.Id,
                };
            }

            public void Update()
            {
                foreach (byte playerId in this.timer.Keys)
                {
                    this.timer[playerId] = this.timer[playerId] += Time.fixedDeltaTime;
                }
            }
        }

        public enum SearchCond
        {
            None = byte.MinValue,
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
        private bool isAssistantReport = false;
        private CrimeInfoContainer info;
        private Arrow crimeArrow;
        private float searchTime;
        private float searchAssistantTime;
        private float timer = 0.0f;
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
            Palette.White,
            false, true, false, false)
        { }

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
            this.isAssistantReport = false;
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            this.targetCrime = this.info.GetCrimeInfo(reportBody.PlayerId);
            this.isAssistantReport = ExtremeRoleManager.GameRole[
                reporter.PlayerId].Id == ExtremeRoleId.Assistant;
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            this.info.AddDeadBody(target);
        }

        public void Update(PlayerControl rolePlayer)
        {

            if (this.prevPlayerPos == null)
            { 
                this.prevPlayerPos = rolePlayer.GetTruePosition();
            }

            if (this.targetCrime != null)
            {
                if (this.crimeArrow == null)
                {
                    this.crimeArrow = new Arrow(
                        new Color32(
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue));
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

                    if ((this.timer > this.searchAssistantTime && this.isAssistantReport) ||
                        (this.timer > this.searchTime))
                    {
                        this.timer = 0.0f;
                        updateSearchCond(crime);
                    }
                    this.timer += Time.fixedDeltaTime;
                }
                else
                {
                    this.timer = 0.0f;
                    resetSearchCond();
                }
            }
            if (this.crimeArrow != null)
            {
                this.crimeArrow.SetActive(false);
            }
            this.prevPlayerPos = rolePlayer.GetTruePosition();
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
                        info.ReportTime);
                    break;
                case SearchCond.FindTeam:
                    showStr = string.Format(
                        Translation.GetString(SearchCond.FindTeam.ToString()),
                        Translation.GetString(info.KillerTeam.ToString()));
                    break;
                case SearchCond.FindRole:
                    showStr = string.Format(
                        Translation.GetString(SearchCond.FindRole.ToString()),
                        Translation.GetString(info.KillerRole.ToString()));
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
    }

    public class Assistant : MultiAssignRoleBase, IRoleMurderPlayerHock, IRoleReportHock
    {
        private Dictionary<byte, DateTime> deadBodyInfo = new Dictionary<byte, DateTime>();
        public Assistant() : base(
            ExtremeRoleId.Assistant,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Assistant.ToString(),
            Palette.White,
            false, true, false, false)
        { }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            this.deadBodyInfo.Add(target.PlayerId, DateTime.Now);
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
                                "reportedDeadBodyInfo",
                                this.deadBodyInfo[reportBody.PlayerId]));
                    }
                }
            }
            this.deadBodyInfo.Clear();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        { }

        protected override void RoleSpecificInit()
        {
            this.deadBodyInfo.Clear();
        }
    }

    public class DetectiveApprentice : SingleRoleBase, IRoleAbility, IRoleReportHock
    {

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
        private int buttonNum;
        private RoleAbilityButtonBase meetingButton;
        private Minigame meeting;

        public DetectiveApprentice() : base(
            ExtremeRoleId.DetectiveApprentice,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.DetectiveApprentice.ToString(),
            Palette.White,
            false, true, false, false)
        { }

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
                "emergencyMeeting",
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
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
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
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
            if (this.useAbility &&
                rolePlayer.PlayerId == reporter.PlayerId &&
                rolePlayer.AmOwner &&
                this.hasOtherButton &&
                this.buttonNum >= 0)
            {
                --this.buttonNum;
                ++rolePlayer.RemainingEmergencies;
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
