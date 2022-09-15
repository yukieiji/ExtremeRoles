using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnhollowerBaseLib.Attributes;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo;

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

            Dictionary <SummaryType, StringBuilder> summary = createSummaryBase();

            for (int i = 0; i < OptionHolder.VanillaMaxPlayerNum; ++i)
            {
                tagColor.Add(
                    UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f, 1f, 1f));
            }

            List<string> randomTag = tags.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            foreach (var playerSummary in getSortedSummary())
            {
                string taskInfo = playerSummary.TotalTask > 0 ?
                    $"<color=#FAD934FF>{playerSummary.CompletedTask}/{playerSummary.TotalTask}</color>" : "";
                string aliveDead = Translation.GetString(
                    playerSummary.StatusInfo.ToString());

                string roleName = playerSummary.Role.GetColoredRoleName(true);
                string tag = playerSummary.Role.GetRoleTag();

                int id = playerSummary.Role.GameControlId;
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

                var mutiAssignRole = playerSummary.Role as MultiAssignRoleBase;
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

                summary[SummaryType.Role].AppendLine(
                    $"{playerSummary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{roleName}");


                GhostRoleBase ghostRole = playerSummary.GhostRole;
                string ghostRoleName = ghostRole != null ?
                    ghostRole.GetColoredRoleName() :
                    Translation.GetString("noGhostRole");

                summary[SummaryType.GhostRole].AppendLine(
                    $"{playerSummary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{ghostRoleName}");
            }

            int allSummary = summary.Count;
            int page = 0;

            foreach (StringBuilder builder in summary.Values)
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

        [HideFromIl2Cpp]
        private List<ExtremeShipStatus.ExtremeShipStatus.PlayerSummary> getSortedSummary()
        {
            var summaryData = ExtremeRolesPlugin.ShipState.FinalSummary;

            summaryData.Sort((x, y) =>
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

            return summaryData;
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
    }
}
