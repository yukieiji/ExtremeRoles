using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using System.Text;

using UnityEngine;



using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Extension;
using System.Diagnostics.CodeAnalysis;
using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.CustomOption;

public sealed class OptionLoadWrapper(in OptionCategory category, int idOffset) : IOptionLoader
{
	private readonly OptionCategory category = category;
	private readonly int idOffset = idOffset;

	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option)
		=> this.TryGet(id + idOffset, out option);
	public IOption Get<T>(T id) where T : Enum
			=> this.category.Get(id.FastInt() + idOffset);

	public bool TryGetValueOption<W, T>(W id, [NotNullWhen(true)] out IValueOption<T>? option)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.category.TryGetValueOption(id.FastInt() + idOffset, out option);
	public bool TryGetValueOption<T>(int id, [NotNullWhen(true)] out IValueOption<T>? option)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.category.TryGetValueOption(id + idOffset, out option);

	public IValueOption<T> GetValueOption<W, T>(W id)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.category.GetValueOption<T>(id.FastInt() + idOffset);
	public IValueOption<T> GetValueOption<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.category.GetValueOption<T>(id + idOffset);

	public T GetValue<W, T>(W id) where W : Enum
		=> this.category.GetValue<T>(id.FastInt() + idOffset);
	public T GetValue<T>(int id)
		=> this.category.GetValue<T>(id + idOffset);
}

public sealed class OptionCategory(
	OptionTab tab,
	int id,
	string name,
	in OptionPack option,
	in Color? color = null) : IOptionLoader
{
	public Color? Color { get; } = color;

	public IEnumerable<IOption> Options => allOpt.Values;
	public int Count => allOpt.Count;

	public OptionTab Tab { get; } = tab;
	public int Id { get; } = id;
	public string Name { get; } = name;
	public string TransedName => Translation.GetString(Name);

	public bool IsDirty { get; set; } = false;

	private readonly IReadOnlyDictionary<int, IValueOption<int>> intOpt = option.IntOptions;
	private readonly IReadOnlyDictionary<int, IValueOption<float>> floatOpt = option.FloatOptions;
	private readonly IReadOnlyDictionary<int, IValueOption<bool>> boolOpt = option.BoolOptions;
	private readonly IReadOnlyDictionary<int, IOption> allOpt = option.AllOptions;

	public void AddHudString(in StringBuilder builder)
	{
		builder.AppendLine($"・OptionCategory: {this.TransedName}");

		foreach (var option in this.allOpt.Values)
		{
			if (!option.IsActiveAndEnable)
			{
				continue;
			}

			builder.AppendLine($"{option.Title}: {option.ValueString}");
		}
	}
	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option)
		=> this.allOpt.TryGetValue(id, out option) && option is not null;
	public IOption Get<T>(T id) where T : Enum
		=> this.Get(id.FastInt());

	public bool TryGetValueOption<W, T>(W id, [NotNullWhen(true)] out IValueOption<T>? option)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.TryGetValueOption(id.FastInt(), out option);

	public bool TryGetValueOption<T>(int id, [NotNullWhen(true)] out IValueOption<T>? option)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
	{
		option = null;
		if (!this.allOpt.ContainsKey(id)) { return false; }

		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOpt[id];
			option = Unsafe.As<IValueOption<int>, IValueOption<T>>(ref intOption);
			return option is not null;
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOpt[id];
			option = Unsafe.As<IValueOption<float>, IValueOption<T>>(ref floatOption);
			return option is not null;
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOpt[id];
			option = Unsafe.As<IValueOption<bool>, IValueOption<T>>(ref boolOption);
			return option is not null;
		}
		else
		{
			throw new ArgumentException("Cannot Find Options");
		}
	}

	public IValueOption<T> GetValueOption<W, T>(W id)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.GetValueOption<T>(id.FastInt());

	public IValueOption<T> GetValueOption<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
	{
		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOpt[id];
			return Unsafe.As<IValueOption<int>, IValueOption<T>>(ref intOption);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOpt[id];
			return Unsafe.As<IValueOption<float>, IValueOption<T>>(ref floatOption);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOpt[id];
			return Unsafe.As<IValueOption<bool>, IValueOption<T>>(ref boolOption);
		}
		else
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
	}

	public IOption Get(int id)
	{
		if (!TryGet(id, out var option))
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
		return option;
	}
	public T GetValue<W, T>(W id) where W : Enum
		=> this.GetValue<T>(id.FastInt());

	public T GetValue<T>(int id)
	{
		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOpt[id];
			int intValue = intOption.Value;
			return Unsafe.As<int, T>(ref intValue);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOpt[id];
			float floatValue = floatOption.Value;
			return Unsafe.As<float, T>(ref floatValue);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOpt[id];
			bool boolValue = boolOption.Value;
			return Unsafe.As<bool, T>(ref boolValue);
		}
		else
		{
			throw new ArgumentException($"OptionId: {typeof(T)} Not Found");
		}
	}
}
