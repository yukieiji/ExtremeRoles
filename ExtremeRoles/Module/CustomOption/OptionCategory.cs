using ExtremeRoles.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using UnityEngine;

using ExtremeRoles.Module.CustomOption.Interfaces;


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

	public T GetValue<W, T>(W id) 
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		where W : Enum
		=> this.category.GetValue<T>(id.FastInt() + idOffset);
	public T GetValue<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
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
	public string TransedName => Tr.GetString(Name);

	public bool IsDirty { get; set; } = false;

	private readonly IReadOnlyDictionary<int, IOption> allOpt = option.AllOptions;

	public void AddHudString(in StringBuilder builder)
	{
		builder.AppendLine($"・{Tr.GetString("OptionCategory")}: {this.TransedName}");

		foreach (var option in this.allOpt.Values)
		{
			if (!option.IsActive)
			{
				continue;
			}

			builder.AppendLine($"{option.TransedTitle}: {option.TransedValue}");
		}
	}
	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option)
		=> this.allOpt.TryGetValue(id, out option) && option is not null;
	public IOption Get<T>(T id) where T : Enum
		=> this.Get(id.FastInt());

	public IOption Get(int id)
	{
		if (!TryGet(id, out var option))
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
		return option;
	}
	public T GetValue<W, T>(W id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		where W : Enum
		=> this.GetValue<T>(id.FastInt());

	public T GetValue<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.Get(id).GetValue<T>();
}
