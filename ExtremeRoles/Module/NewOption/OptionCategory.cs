using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using System.Text;

using OptionTab = ExtremeRoles.Module.CustomOption.OptionTab;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Extension;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionCategory(
	OptionTab tab,
	int id,
	string name,
	in OptionPack option)
{
	public IEnumerable<IOption> Options => allOpt.Values;
	public int Count => allOpt.Count;

	public OptionTab Tab { get; } = tab;
	public int Id { get; } = id;
	public string Name { get; } = name;
	public bool IsDirty { get; set; } = false;

	private readonly ImmutableDictionary<int, IValueOption<int>> intOpt = option.IntOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IValueOption<float>> floatOpt = option.FloatOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IValueOption<bool>> boolOpt = option.BoolOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IOption> allOpt = option.AllOptions.ToImmutableDictionary();

	public void AddHudString(in StringBuilder builder)
	{
		builder.Append($"・OptionCategory: {this.Name}");

		foreach (var option in this.allOpt.Values)
		{
			if (!option.IsActiveAndEnable)
			{
				continue;
			}

			builder.AppendLine($"{option.Title}: {option.ValueString}");
		}
	}

	public bool TryGet(int id, out IOption option)
		=> this.allOpt.TryGetValue(id, out option) && option is not null;
	public IOption Get<T>(T id) where T : Enum
		=> this.Get(id.FastInt());

	public IValueOption<T> GetValueOption<W, T>(W id) where W : Enum
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
			return default(T);
		}
	}
}
