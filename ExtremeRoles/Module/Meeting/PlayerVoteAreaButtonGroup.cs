using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.Meeting;

public sealed class PlayerVoteAreaButtonGroup
{
	private readonly List<PlayerVoteAreaButtonPostionComputer> first = new(2);
	private readonly List<PlayerVoteAreaButtonPostionComputer> second = new();

	private const float yOffset = 0.25f;

	public PlayerVoteAreaButtonGroup(PlayerVoteArea __instance)
	{
		this.AddFirstRow(__instance.CancelButton);
		this.AddFirstRow(__instance.ConfirmButton);
	}

	public void ResetFirst()
	{
		if (this.first.Count <= 2)
		{
			return;
		}
		this.first.RemoveRange(3, this.first.Count + 1 - 3);
	}

	public void ResetSecond()
		=> this.second.Clear();

	public IEnumerable<IPlayerVoteAreaButtonPostionComputer> Flatten(float startPos)
	{
		int secondCount = this.second.Count;

		var result = new List<PlayerVoteAreaButtonPostionComputer>(secondCount + this.first.Count);

		var firstOffset = secondCount > 0 ? Vector2.up * yOffset : Vector2.zero;
		var secondOffset = secondCount > 0 ? Vector2.down * yOffset : Vector2.zero;

		foreach (var buttn in setUpComputer(this.first, firstOffset, startPos))
		{
			yield return buttn;
		}
		foreach (var buttn in setUpComputer(this.second, secondOffset, startPos))
		{
			yield return buttn;
		}
	}

	public IEnumerable<IPlayerVoteAreaButtonPostionComputer> DefaultFlatten(float startPos)
		=> setUpComputer(this.first.GetRange(0, 2), Vector2.zero, startPos);

	public void AddFirstRow(UiElement element)
	{
		Logging.Debug($"Add first row : {element.name}");
		add(this.first, element);
	}

	public void AddSecondRow(UiElement element)
	{
		Logging.Debug($"Add second row : {element.name}");
		add(this.second, element);
	}

	private static IEnumerable<IPlayerVoteAreaButtonPostionComputer> setUpComputer(
		IEnumerable<PlayerVoteAreaButtonPostionComputer> setUpContainer,
		Vector2 offset, float statPos)
	{
		foreach (var button in setUpContainer)
		{
			button.Offset = offset;
			button.StartOffset = statPos;
			Logging.Debug($"Meeting Button[{button.ToString()}]");
			yield return button;
		}
	}

	private static void add(in List<PlayerVoteAreaButtonPostionComputer> groups, UiElement element)
	{
		int size = groups.Count;
		float time = (size * 0.1f) + 0.25f;
		float endOffset = 1.3f - (size * 0.65f);
		if (endOffset <= 0.0f)
		{
			endOffset = -0.01f;
		}
		groups.Add(new PlayerVoteAreaButtonPostionComputer(time, element, endOffset));
	}
}
