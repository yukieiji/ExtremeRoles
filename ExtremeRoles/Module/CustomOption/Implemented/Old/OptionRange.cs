using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented.Old;

public class OptionRange<T>(T[] option) : IOptionRange<T>
	where T :
		notnull, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
{
	public T Value => _option[Selection];
	public T Min => _option[0];
	public T Max => _option[Range - 1];

	public int Range => _option.Length;

	public int Selection
	{
		get => _selection;
		set
		{
			int length = Range;
			int clampedNewValue = Mathf.Clamp(
				(value + length) % length,
				0, length - 1);

			_selection = clampedNewValue;
		}
	}
	private readonly T[] _option = option;
	private int _selection = 0;

	private OptionRange(IEnumerable<T> range) : this(range.ToArray())
	{ }

	public int GetIndex(T value)
	{
		int index = Array.IndexOf(_option, value);
		return Math.Max(0, index);
	}

	public override string ToString()
		=> $"Cur:{Value} (Min:{Min}, Max:{Max}, Selected Index:{Selection})";

	public static OptionRange<int> Create(int min, int max, int step)
	{
		var range = GetIntRange(min, max, step);
		return new OptionRange<int>(range);
	}
	public static OptionRange<float> Create(float min, float max, float step)
	{
		var range = GetFloatRange(min, max, step);
		return new OptionRange<float>(range);
	}
	public static OptionRange<string> Create<W>() where W : struct, Enum
	{
		var range = GetEnumString<W>();
		return new OptionRange<string>(range);
	}

	private static IEnumerable<string> GetEnumString<W>() where W : struct, Enum
	{
		foreach (W enumValue in Enum.GetValues<W>())
		{
			string? valuse = enumValue.ToString();
			if (string.IsNullOrEmpty(valuse)) { continue; }

			yield return valuse;
		}
	}

	private static IEnumerable<int> GetIntRange(int min, int max, int step)
	{
		for (int s = min; s <= max; s += step)
		{
			yield return s;
		}
	}

	private static IEnumerable<float> GetFloatRange(float min, float max, float step)
	{
		decimal dStep = new decimal(step);
		decimal dMin = new decimal(min);
		decimal dMax = new decimal(max);

		for (decimal s = dMin; s <= dMax; s += dStep)
		{
			yield return (float)decimal.ToDouble(s);
		}
	}
}
