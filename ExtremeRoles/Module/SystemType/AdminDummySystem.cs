using ExtremeRoles.Module.Interface;
using Hazel;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class AdminDummySystem : IExtremeSystemType
{
	public bool IsActive => this.colors.Count > 0;

	public DummyMode Mode { get; set; } = DummyMode.Add;

	public enum DummyMode
	{
		Add,
		Override,
	}

	public enum Option
	{
		Add,
		Remove,
	}

	private readonly Dictionary<SystemTypes, List<int>> colors = new Dictionary<SystemTypes, List<int>>();

	public static AdminDummySystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<AdminDummySystem>(ExtremeSystemType.AdminDummySystem);

	public static bool TryGet([NotNullWhen(true)] out AdminDummySystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.AdminDummySystem, out system);

	public void Add(SystemTypes room, params int[] color)
	{
		lock (colors)
		{
			foreach (int c in color)
			{
				if (!this.colors.TryGetValue(room, out var curColor))
				{
					curColor = new List<int>();
					this.colors[room] = curColor;
				}
				curColor.Add(c);
			}
		}
	}

	public void Remove(SystemTypes room, params int[] color)
	{
		lock (colors)
		{
			foreach (int c in color)
			{
				if (!this.colors.TryGetValue(room, out var curColor))
				{
					curColor = new List<int>();
					this.colors[room] = curColor;
				}
				curColor.Add(c);
			}
		}
	}

	public void Remove(SystemTypes room, IReadOnlyList<int> color)
	{
		lock (colors)
		{
			foreach (int c in color)
			{
				if (!this.colors.TryGetValue(room, out var curColor))
				{
					curColor = new List<int>();
					this.colors[room] = curColor;
				}
				curColor.Add(c);
			}
		}
	}

	public bool TryGet(SystemTypes room, [NotNullWhen(true)] out IReadOnlyList<int>? colors)
	{
		bool result = this.colors.TryGetValue(room, out var rowColors);
		colors = rowColors;
		return result;
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.OnPlayer)
		{
			this.colors.Clear();
		}
	}

	public void UpdateSystem(
		PlayerControl player,
		MessageReader msgReader)
	{
		var opt = (Option)msgReader.ReadByte();
		var room = (SystemTypes)msgReader.ReadByte();

		switch (opt)
		{
			case Option.Add:
				int color = msgReader.ReadPackedInt32();
				this.Add(room, color);
				break;
			case Option.Remove:
				this.Remove(room);
				break;
			default:
				break;
		}

	}
}
