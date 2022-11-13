using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnhollowerBaseLib.Attributes;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo;

using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class FinalSummary : MonoBehaviour
    {
        public enum SummaryType
        {
            Role,
            GhostRole
        }

        private List<string> summaryText = new List<string>();
        private int curPage;
        private int maxPage;

        private TMP_Text showText;
        private RectTransform rect;
        private bool isCreated = false;
        private bool isHide = false;

        private static List<string> tags = new List<string>()
        {
            "γ", "ζ", "δ", "ε", "η",
            "θ", "λ", "μ", "π", "ρ",
            "σ", "φ", "ψ", "χ", "ω"
        };

        public FinalSummary(IntPtr ptr) : base(ptr) { }

        private static List<PlayerSummary> playerSummary = new List<PlayerSummary>();

        public static void Add(GameData.PlayerInfo playerInfo)
        {
            byte playerId = playerInfo.PlayerId;

            SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
            var (completedTask, totalTask) = GameSystem.GetTaskInfo(playerInfo);
            // IsImpostor

            PlayerStatus finalStatus = PlayerStatus.Alive;
            GameOverReason reson = ExtremeRolesPlugin.ShipState.EndReason;
            Dictionary<byte, DeadInfo> info = ExtremeRolesPlugin.ShipState.DeadPlayerInfo;

            if (info.TryGetValue(playerId, out DeadInfo deadInfo))
            {
                finalStatus = deadInfo.Reason;
            }

            if (reson == GameOverReason.ImpostorBySabotage &&
                !role.IsImpostor())
            {
                finalStatus = PlayerStatus.Dead;
            }
            else if (reson == (GameOverReason)RoleGameOverReason.AssassinationMarin)
            {
                if (ExtremeRolesPlugin.ShipState.isMarinPlayer(playerId))
                {
                    if (playerInfo.IsDead || playerInfo.Disconnected)
                    {
                        finalStatus = PlayerStatus.DeadAssassinate;
                    }
                    else
                    {
                        finalStatus = PlayerStatus.Assassinate;
                    }
                }
                else if (
                    !role.IsImpostor() &&
                    !playerInfo.IsDead &&
                    !playerInfo.Disconnected)
                {
                    finalStatus = PlayerStatus.Surrender;
                }
            }
            else if (reson == (GameOverReason)RoleGameOverReason.UmbrerBiohazard)
            {
                if (role.Id != ExtremeRoleId.Umbrer &&
                    !playerInfo.IsDead &&
                    !playerInfo.Disconnected)
                {
                    finalStatus = PlayerStatus.Zombied;
                }
            }
            else if (playerInfo.Disconnected)
            {
                finalStatus = PlayerStatus.Disconnected;
            }

            ExtremeGhostRoleManager.GameRole.TryGetValue(
                playerId, out GhostRoleBase ghostRole);

            playerSummary.Add(
                new PlayerSummary
                {
                    PlayerName = playerInfo.PlayerName,
                    Role = role,
                    GhostRole = ghostRole,
                    StatusInfo = finalStatus,
                    TotalTask = totalTask,
                    CompletedTask = reson == GameOverReason.HumansByTask ? totalTask : completedTask,
                });
        }

        public static List<PlayerSummary> GetSummary() => playerSummary;

        public static void Reset()
        {
            playerSummary.Clear();
        }

        public void Awake()
        {
            this.showText = GetComponent<TMP_Text>();
            this.showText.alignment = TextAlignmentOptions.TopLeft;
            this.showText.color = Color.white;
            this.showText.outlineWidth *= 1.2f;
            this.showText.fontSizeMin = 1.25f;
            this.showText.fontSizeMax = 1.25f;
            this.showText.fontSize = 1.25f;

            this.rect = this.showText.GetComponent<RectTransform>();

            this.summaryText.Clear();
            this.curPage = 0;
            this.isCreated = false;
        }

        public void Update()
        {

            if (!this.isCreated) { return; }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                this.curPage = (this.curPage + 1) % this.maxPage;
                updateShowText();
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (this.isHide)
                {
                    this.isHide = false;
                    updateShowText();
                }
                else
                {
                    this.isHide = true;
                    this.showText.text = Translation.GetString("liftShiftShowSummary");
                }
            }
        }

        public void Create()
        {
            List<Color> tagColor = new List<Color>();

            Dictionary <SummaryType, StringBuilder> finalSummary = createSummaryBase();

            for (int i = 0; i < OptionHolder.VanillaMaxPlayerNum; ++i)
            {
                tagColor.Add(
                    UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f, 1f, 1f));
            }

            List<string> randomTag = tags.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            sortedSummary();

            foreach (PlayerSummary summary in playerSummary)
            {
                string taskInfo = summary.TotalTask > 0 ?
                    $"<color=#FAD934FF>{summary.CompletedTask}/{summary.TotalTask}</color>" : "";
                string aliveDead = Translation.GetString(
                    summary.StatusInfo.ToString());

                string roleName = summary.Role.GetColoredRoleName(true);
                string tag = summary.Role.GetRoleTag();

                int id = summary.Role.GameControlId;
                int index = id % OptionHolder.VanillaMaxPlayerNum;
                if (tag != string.Empty)
                {
                    tag = Design.ColoedString(
                        tagColor[index], tag);
                }
                else
                {
                    tag = Design.ColoedString(
                        tagColor[index], randomTag[index]);
                }

                var mutiAssignRole = summary.Role as MultiAssignRoleBase;
                if (mutiAssignRole != null)
                {
                    if (mutiAssignRole.AnotherRole != null)
                    {
                        string anotherTag = mutiAssignRole.AnotherRole.GetRoleTag();
                        id = mutiAssignRole.AnotherRole.GameControlId;
                        index = id % OptionHolder.VanillaMaxPlayerNum;

                        if (anotherTag != string.Empty)
                        {
                            anotherTag = Design.ColoedString(
                                tagColor[index], anotherTag);
                        }
                        else
                        {
                            anotherTag = Design.ColoedString(
                                tagColor[index], randomTag[index]);
                        }

                        tag = string.Concat(
                            tag, " + ", anotherTag);

                    }
                }

                finalSummary[SummaryType.Role].AppendLine(
                    $"{summary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{roleName}");


                GhostRoleBase ghostRole = summary.GhostRole;
                string ghostRoleName = ghostRole != null ?
                    ghostRole.GetColoredRoleName() :
                    Translation.GetString("noGhostRole");

                finalSummary[SummaryType.GhostRole].AppendLine(
                    $"{summary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{ghostRoleName}");
            }

            int allSummary = finalSummary.Count;
            int page = 0;

            foreach (StringBuilder builder in finalSummary.Values)
            {
                ++page;
                builder.AppendLine("");
                builder.AppendLine(string.Format(
                    Translation.GetString("tabMoreSummary"), page, allSummary));
                builder.AppendLine(Translation.GetString("liftShiftHideSummary"));
                this.summaryText.Add(builder.ToString());
            }
            this.maxPage = page;
            this.isCreated = true;
            updateShowText();
        }

        public void SetAnchorPoint(Vector2 position)
        {
            this.rect.anchoredPosition = new Vector2(
                position.x + 3.5f, position.y - 0.1f);
        }

        private void sortedSummary()
        {
            playerSummary.Sort((x, y) =>
            {
                if (x.StatusInfo != y.StatusInfo)
                {
                    return x.StatusInfo.CompareTo(y.StatusInfo);
                }

                if (x.Role.Id != y.Role.Id)
                {
                    return x.Role.Id.CompareTo(y.Role.Id);
                }
                if (x.Role.Id == ExtremeRoleId.VanillaRole)
                {
                    var xVanillaRole = (VanillaRoleWrapper)x.Role;
                    var yVanillaRole = (VanillaRoleWrapper)y.Role;

                    return xVanillaRole.VanilaRoleId.CompareTo(
                        yVanillaRole.VanilaRoleId);
                }

                return x.PlayerName.CompareTo(y.PlayerName);

            });
        }

        [HideFromIl2Cpp]
        private Dictionary<SummaryType, StringBuilder> createSummaryBase()
        {
            Dictionary<SummaryType, StringBuilder> summary = new Dictionary<SummaryType, StringBuilder>()
            {
                { SummaryType.Role, new StringBuilder() },
                { SummaryType.GhostRole, new StringBuilder() },
            };
            
            foreach (var (type, builder) in summary)
            {
                switch (type)
                {
                    case SummaryType.Role:
                        builder.AppendLine(Translation.GetString("summaryText"));
                        builder.AppendLine(Translation.GetString("roleSummaryInfo"));
                        break;
                    case SummaryType.GhostRole:
                        builder.AppendLine(Translation.GetString("summaryText"));
                        builder.AppendLine(Translation.GetString("ghostRoleSummaryInfo"));
                        break;
                    default:
                        break;
                }
            }

            return summary;
        }

        private void updateShowText()
        {
            this.showText.text = this.summaryText[this.curPage];
        }

        public sealed class PlayerSummary
        {
            public string PlayerName { get; set; }
            public SingleRoleBase Role { get; set; }
            public GhostRoleBase GhostRole { get; set; }
            public int CompletedTask { get; set; }
            public int TotalTask { get; set; }
            public PlayerStatus StatusInfo { get; set; }
        }
    }
}
