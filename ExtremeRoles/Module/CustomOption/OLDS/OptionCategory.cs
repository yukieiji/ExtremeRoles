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

namespace ExtremeRoles.Module.CustomOption.OLDS;

public sealed class OldOptionLoadWrapper(in OldOptionCategory category, int idOffset) : IOptionLoader
{
	private readonly OldOptionCategory category = category;
	private readonly int idOffset = idOffset;

	public bool TryGet(int id, [NotNullWhen(true)] out IOldOption? option)
		=> TryGet(id + idOffset, out option);
	public IOldOption Get<T>(T id) where T : Enum
			=> category.Get(id.FastInt() + idOffset);

	public bool TryGetValueOption<W, T>(W id, [NotNullWhen(true)] out IOldValueOption<T>? option)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> category.TryGetValueOption(id.FastInt() + idOffset, out option);
	public bool TryGetValueOption<T>(int id, [NotNullWhen(true)] out IOldValueOption<T>? option)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> category.TryGetValueOption(id + idOffset, out option);

	public IOldValueOption<T> GetValueOption<W, T>(W id)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> category.GetValueOption<T>(id.FastInt() + idOffset);
	public IOldValueOption<T> GetValueOption<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> category.GetValueOption<T>(id + idOffset);

	public T GetValue<W, T>(W id) where W : Enum
		=> category.GetValue<T>(id.FastInt() + idOffset);
	public T GetValue<T>(int id)
		=> category.GetValue<T>(id + idOffset);
}

public sealed class OldOptionCategory(
	OptionTab tab,
	int id,
	string name,
	in OldOptionPack option,
	in Color? color = null) : IOptionLoader
{
	public Color? Color { get; } = color;

	public IEnumerable<IOldOption> Options => allOpt.Values;
	public int Count => allOpt.Count;

	public OptionTab Tab { get; } = tab;
	public int Id { get; } = id;
	public string Name { get; } = name;
	public string TransedName => Tr.GetString(Name);

	public bool IsDirty { get; set; } = false;

	private readonly IReadOnlyDictionary<int, IOldValueOption<int>> intOpt = option.IntOptions;
	private readonly IReadOnlyDictionary<int, IOldValueOption<float>> floatOpt = option.FloatOptions;
	private readonly IReadOnlyDictionary<int, IOldValueOption<bool>> boolOpt = option.BoolOptions;
	private readonly IReadOnlyDictionary<int, IOldOption> allOpt = option.AllOptions;

	public void AddHudString(in StringBuilder builder)
	{
		builder.AppendLine($"・{Tr.GetString("OptionCategory")}: {TransedName}");

		foreach (var option in allOpt.Values)
		{
			if (!option.IsActiveAndEnable)
			{
				continue;
			}

			builder.AppendLine($"{option.Title}: {option.ValueString}");
		}
	}
	public bool TryGet(int id, [NotNullWhen(true)] out IOldOption? option)
		=> allOpt.TryGetValue(id, out option) && option is not null;
	public IOldOption Get<T>(T id) where T : Enum
		=> Get(id.FastInt());

	public bool TryGetValueOption<W, T>(W id, [NotNullWhen(true)] out IOldValueOption<T>? option)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> TryGetValueOption(id.FastInt(), out option);

	public bool TryGetValueOption<T>(int id, [NotNullWhen(true)] out IOldValueOption<T>? option)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
	{
		option = null;
		if (!allOpt.ContainsKey(id)) { return false; }

		if (typeof(T) == typeof(int))
		{
			var intOption = intOpt[id];
			option = Unsafe.As<IOldValueOption<int>, IOldValueOption<T>>(ref intOption);
			return option is not null;
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = floatOpt[id];
			option = Unsafe.As<IOldValueOption<float>, IOldValueOption<T>>(ref floatOption);
			return option is not null;
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = boolOpt[id];
			option = Unsafe.As<IOldValueOption<bool>, IOldValueOption<T>>(ref boolOption);
			return option is not null;
		}
		else
		{
			throw new ArgumentException("Cannot Find Options");
		}
	}

	public IOldValueOption<T> GetValueOption<W, T>(W id)
		where W : Enum
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> GetValueOption<T>(id.FastInt());

	public IOldValueOption<T> GetValueOption<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
	{
		if (typeof(T) == typeof(int))
		{
			var intOption = intOpt[id];
			return Unsafe.As<IOldValueOption<int>, IOldValueOption<T>>(ref intOption);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = floatOpt[id];
			return Unsafe.As<IOldValueOption<float>, IOldValueOption<T>>(ref floatOption);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = boolOpt[id];
			return Unsafe.As<IOldValueOption<bool>, IOldValueOption<T>>(ref boolOption);
		}
		else
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
	}

	public IOldOption Get(int id)
	{
		if (!TryGet(id, out var option))
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
		return option;
	}
	public T GetValue<W, T>(W id) where W : Enum
		=> GetValue<T>(id.FastInt());

	public T GetValue<T>(int id)
	{
		if (typeof(T) == typeof(int))
		{
			var intOption = intOpt[id];
			int intValue = intOption.Value;
			return Unsafe.As<int, T>(ref intValue);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = floatOpt[id];
			float floatValue = floatOption.Value;
			return Unsafe.As<float, T>(ref floatValue);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = boolOpt[id];
			bool boolValue = boolOption.Value;
			return Unsafe.As<bool, T>(ref boolValue);
		}
		else
		{
			throw new ArgumentException($"OptionId: {typeof(T)} Not Found");
		}
	}
}
