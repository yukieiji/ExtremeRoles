using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Implemented;

public class DynamismOptionRange<T>(IOptionRange<T> range)
	: IOptionRange<T>
	where T :
		notnull, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
{
	public IOptionRange<T> InnerRange { get; set; } = range;

	public T RangedValue => InnerRange.RangedValue;
	public T Min => InnerRange.Min;
	public T Max => InnerRange.Max;

	public int Range => InnerRange.Range;

	public int Selection
	{
		get => InnerRange.Selection;
		set
		{
			InnerRange.Selection = value;
		}
	}

	public event Action OnValueChanged
	{
		add
		{
			this.InnerRange.OnValueChanged += value;
		}
		remove
		{
			this.InnerRange.OnValueChanged -= value;
		}
	}

	public int GetIndex(T value)
		=> InnerRange.GetIndex(value);

	public override string ToString()
		=> InnerRange.ToString();
}

public class OptionRange<T>(T[] option) : IOptionRange<T>
	where T :
		notnull, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
{
	public T RangedValue => option[Selection];
	public T Min => option[0];
	public T Max => option[Range - 1];

	public int Range => option.Length;

	public int Selection
	{
		get => selection;
		set
		{
			int length = Range;
			int clampedNewValue = Mathf.Clamp(
				(value + length) % length,
				0, length - 1);

			selection = clampedNewValue;
			this.onValueChanged?.Invoke();
		}
	}
	private readonly T[] option = option;
	private int selection = 0;

	public event Action OnValueChanged
	{
		add
		{
			this.onValueChanged += value;
			value.Invoke();
		}
		remove
		{
			this.onValueChanged -= value;
		}
	}
	private Action? onValueChanged;

	public OptionRange(IEnumerable<T> range) : this(range.ToArray())
	{ }

	public void TransferNewRange(OptionRange<T> @new)
	{
		@new.onValueChanged = this.onValueChanged;
		this.onValueChanged = null;
	}

	public int GetIndex(T value)
	{
		int index = Array.IndexOf(option, value);
		return Math.Max(0, index);
	}

	public override string ToString()
		=> $"Cur:{RangedValue} (Min:{Min}, Max:{Max}, Selected Index:{Selection})";

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

	public static IEnumerable<string> GetEnumString<W>() where W : struct, Enum
	{
		foreach (W enumValue in Enum.GetValues<W>())
		{
			string? valuse = enumValue.ToString();
			if (string.IsNullOrEmpty(valuse)) { continue; }

			yield return valuse;
		}
	}

	public static IEnumerable<int> GetIntRange(int min, int max, int step)
	{
		for (int s = min; s <= max; s += step)
		{
			yield return s;
		}
	}

	public static IEnumerable<float> GetFloatRange(float min, float max, float step)
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
