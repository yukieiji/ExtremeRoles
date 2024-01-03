using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

using UnityEngine;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class IngameTextShower : NullableSingleton<IngameTextShower>
{
	private TextMeshPro? infoText;
	private readonly StringBuilder builder = new StringBuilder();
	private readonly List<Func<string>> prevStringBuildAction = new List<Func<string>>();

	public void RebuildPingString(in PingTracker tracker)
	{
		tracker.text.alignment = TextAlignmentOptions.TopRight;

		this.builder.Clear();
		foreach (var act in this.prevStringBuildAction)
		{
			string str = act.Invoke();
			this.builder.AppendLine(str);
		}
		this.builder.Append(tracker.text.text);

		tracker.text.text = this.builder.ToString();
	}

	public void RebuildVersionShower(in VersionShower versionShower)
	{
		if (this.infoText == null)
		{
			this.infoText = UnityObject.Instantiate(versionShower.text);
			this.infoText.name = "ExtremeRolesInfoText";
			this.infoText.text = string.Empty;
			this.infoText.alignment = TextAlignmentOptions.TopRight;
			this.infoText.fontSize = this.infoText.fontSizeMax = this.infoText.fontSizeMin = 1.9f;

			AspectPosition aspectPosition = this.infoText.gameObject.AddComponent<AspectPosition>();
			aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
			aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
			aspectPosition.DistanceFromEdge = new Vector3(2.1f, 1.225f, -10f);
			aspectPosition.AdjustPosition();
		}

		this.infoText.text = string.Empty;
		setText(this.infoText);
	}
	public void RebuildVersionShower()
	{
		if (this.infoText == null) { return; }

		this.infoText.text = string.Empty;
		setText(this.infoText);
	}

	public void Add(Func<string> strBuildAct)
	{
		this.prevStringBuildAction.Add(strBuildAct);
	}

	public void Add(string str)
	{
		this.Add(() => str);
	}

	private void setText(in TextMeshPro text)
	{
		if (this.prevStringBuildAction.Count == 0) { return; }

		this.builder.Clear();
		foreach (var act in this.prevStringBuildAction)
		{
			string str = act.Invoke();

			if (string.IsNullOrEmpty(str)) { continue; }
			
			this.builder.AppendLine(str);
		}
		this.builder.Append(text.text);

		text.text = this.builder.ToString();

	}
}
