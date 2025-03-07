﻿using System;
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
		return Design.ColoedString(randomColor[index], tag);
	}

	private static IEnumerable<Color> createRandomColor(int size)
	{
		for (int i = 0; i < size; ++i)
		{
			yield return UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f, 1f, 1f);
		}
	}
}

[Il2CppRegister]
public sealed class FinalSummary : MonoBehaviour
{
	public enum SummaryType
	{
		Role,
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
		Dictionary<SummaryType, StringBuilder> finalSummary = createSummaryBase();
		var painter = new TagPainter(tags);
		var sorted = sortedSummary(summaries);

		foreach (PlayerSummary summary in sorted)
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

			finalSummary[SummaryType.Role].AppendLine(
				$"{summary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{roleName}");


			GhostRoleBase ghostRole = summary.GhostRole;
			string ghostRoleName = ghostRole != null ?
				ghostRole.GetColoredRoleName() :
				Tr.GetString("noGhostRole");

			finalSummary[SummaryType.GhostRole].AppendLine(
				$"{summary.PlayerName}<pos=18%>{taskInfo}<pos=27%>{aliveDead}<pos=35%>{tag}:{ghostRoleName}");
		}

		int allSummary = finalSummary.Count;
		int page = 0;

		foreach (StringBuilder builder in finalSummary.Values)
		{
			++page;
			builder.AppendLine();
			builder.AppendLine(Tr.GetString("tabMoreSummary", page, allSummary));
			builder.AppendLine(Tr.GetString("shiftHideSummary"));
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
	private PlayerSummary[] sortedSummary(IReadOnlyList<PlayerSummary> summaries)
	{
		var arr = summaries.ToArray();
		Array.Sort(arr, (x, y) =>
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
		return arr;
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
					builder.AppendLine(Tr.GetString("summaryText"));
					builder.AppendLine(Tr.GetString("roleSummaryInfo"));
					break;
				case SummaryType.GhostRole:
					builder.AppendLine(Tr.GetString("summaryText"));
					builder.AppendLine(Tr.GetString("ghostRoleSummaryInfo"));
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

	public readonly record struct PlayerSummary(
		byte PlayerId,
		string PlayerName,
		SingleRoleBase Role,
		GhostRoleBase GhostRole,
		int CompletedTask,
		int TotalTask,
		PlayerStatus StatusInfo);
}
