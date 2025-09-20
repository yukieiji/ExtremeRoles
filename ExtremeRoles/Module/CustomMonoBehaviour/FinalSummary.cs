using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo;
using ExtremeRoles.Module.GameResult;

using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

public sealed class TagPainter(string[] tags)
{
	private readonly string[] randomTags = tags.OrderBy(
		x => RandomGenerator.Instance.Next()).ToArray();
	private readonly Color[] randomColor = createRandomColor(tags.Length).ToArray();
	private readonly int size = tags.Length;

	public string Paint(string tag, int controlId)
	{
		int index = controlId % size;
		tag = tag != string.Empty ? tag : randomTags[index];
		return Design.ColoredString(randomColor[index], tag);
	}

	private static IEnumerable<Color> createRandomColor(int size)
	{
		for (int i = 0; i < size; ++i)
		{
			yield return UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f, 1f, 1f);
		}
	}
}

public sealed class SummaryTextBuilder
{
	private readonly StringBuilder builder = new StringBuilder();
	public SummaryTextBuilder(string headerKey)
	{
		this.builder.AppendLine(
			Tr.GetString("summaryText"));
		this.builder.AppendLine(Tr.GetString(headerKey));
	}

	public override string ToString()
		=> this.builder.ToString();

	public void AppendLine()
	{
		this.builder.AppendLine();
	}

	public void AppendLine(string text)
	{
		this.builder.AppendLine(text);
	}

	public void AppendFooter(int page, int allPage)
	{
		this.builder.AppendLine();
		this.builder.AppendLine(Tr.GetString("tabMoreSummary", page, allPage));
		this.builder.AppendLine(Tr.GetString("shiftHideSummary"));
	}
}

[Il2CppRegister]
public sealed class FinalSummary : MonoBehaviour
{
	public enum SummaryType
	{
		Role,
		RoleHistory,
		GhostRole
	}

	private readonly List<string> summaryText = new List<string>();
	private int curPage;
	private int maxPage;

	private TMP_Text showText;
	private RectTransform rect;
	private bool isCreated = false;
	private bool isHide = false;

	private readonly string[] tags =
	[
		"γ", "ζ", "δ", "ε", "η",
		"θ", "λ", "μ", "π", "ρ",
		"σ", "φ", "ψ", "χ", "ω"
	];

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
		if (!this.isCreated)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Tab))
		{
			this.curPage = (this.curPage + 1) % this.maxPage;
			updateShowText();
		}
		if (Key.IsShiftDown())
		{
			if (this.isHide)
			{
				this.isHide = false;
				updateShowText();
			}
			else
			{
				this.isHide = true;
				this.showText.text = Tr.GetString("shiftShowSummary");
			}
		}
	}

	[HideFromIl2Cpp]
	public void Create(IReadOnlyList<PlayerSummary> summaries)
	{
		var finalSummary = createSummaryBase();
		var painter = new TagPainter(tags);
		var sorted = sortedSummary(summaries);

		foreach (PlayerSummary summary in sorted.Values)
		{
			string taskInfo = summary.TotalTask > 0 ?
				$"<color=#FAD934FF>{summary.CompletedTask}/{summary.TotalTask}</color>" : "";
			string aliveDead = Tr.GetString(
				summary.StatusInfo.ToString());

			string roleName = summary.Role.GetColoredRoleName(true);
			string tag = painter.Paint(
				summary.Role.GetRoleTag(),
				summary.Role.GameControlId);

			if (summary.Role is MultiAssignRoleBase mutiAssignRole &&
				mutiAssignRole?.AnotherRole != null)
			{
				string anotherTag = painter.Paint(
					mutiAssignRole.AnotherRole.GetRoleTag(),
					mutiAssignRole.AnotherRole.GameControlId);

				tag = $"{tag} + {anotherTag}";
			}

			if (finalSummary.TryGetValue(SummaryType.Role, out var roleSummary) &&
				roleSummary is not null)
			{
				roleSummary.AppendLine(
					$"{summary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{roleName}");
			}


			GhostRoleBase ghostRole = summary.GhostRole;
			string ghostRoleName = ghostRole != null ?
				ghostRole.Visual.ColoredRoleName :
				Tr.GetString("noGhostRole");

			if (finalSummary.TryGetValue(SummaryType.GhostRole, out var ghostRoleSummary) &&
				ghostRoleSummary is not null)
			{
				ghostRoleSummary.AppendLine(
					$"{summary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{ghostRoleName}");
			}
		}

		if (finalSummary.TryGetValue(SummaryType.RoleHistory, out var roleHistorySummary) &&
			roleHistorySummary is not null)
		{
			using var historyBuilder = RoleHistoryContainer.CreateBuiler(roleHistorySummary);
			historyBuilder.Build(sorted);
		}

		int allSummary = finalSummary.Count;
		int page = 0;

		foreach (var builder in finalSummary.Values)
		{
			++page;
			builder.AppendFooter(page, allSummary);
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
	private IReadOnlyDictionary<byte, PlayerSummary> sortedSummary(IReadOnlyList<PlayerSummary> summaries)
	{
		var arr = summaries.ToArray();
		Array.Sort(arr, (x, y) =>
		{
			if (x.StatusInfo != y.StatusInfo)
			{
				return x.StatusInfo.CompareTo(y.StatusInfo);
			}

			var xId = x.Role.Core.Id;
			var yId = y.Role.Core.Id;
			if (xId != yId)
			{
				return xId.CompareTo(yId);
			}
			if (xId == ExtremeRoleId.VanillaRole)
			{
				var xVanillaRole = (VanillaRoleWrapper)x.Role;
				var yVanillaRole = (VanillaRoleWrapper)y.Role;

				return xVanillaRole.VanilaRoleId.CompareTo(
					yVanillaRole.VanilaRoleId);
			}

			return x.PlayerName.CompareTo(y.PlayerName);

		});
		return arr.ToDictionary(x => x.PlayerId);
	}

	[HideFromIl2Cpp]
	private IReadOnlyDictionary<SummaryType, SummaryTextBuilder> createSummaryBase()
		=> new Dictionary<SummaryType, SummaryTextBuilder>()
		{
			{ SummaryType.Role, new SummaryTextBuilder("roleSummaryInfo") },
			{ SummaryType.RoleHistory, new SummaryTextBuilder("roleHistorySummary") },
			{ SummaryType.GhostRole, new SummaryTextBuilder("ghostRoleSummaryInfo") },
		};

	private void updateShowText()
	{
		this.showText.text = this.summaryText[this.curPage];
	}

	public readonly record struct PlayerSummary(
		byte PlayerId,
		string PlayerName,
		SingleRoleBase Role,
		GhostRoleBase GhostRole,
		int CompletedTask,
		int TotalTask,
		PlayerStatus StatusInfo);
}
