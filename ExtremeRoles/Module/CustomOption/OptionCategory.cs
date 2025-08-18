using ExtremeRoles.Extension;
using ExtremeRoles.Module.CustomOption.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using System.Text;

#nullable enable

namespace ExtremeRoles.Module.CustomOption;

public readonly record struct OptionPack(
	OptionTab Tab,
	int Id,
	string Name,
	IReadOnlyDictionary<int, IOption> Option,
	Color? color = null);

public sealed class OptionCategory(
	OptionPack option)
{
	public Color? Color { get; } = option.color;

	public IEnumerable<IOption> Options => allOpt.Values;
	public int Count => allOpt.Count;

	public OptionTab Tab { get; } = option.Tab;
	public int Id { get; } = option.Id;
	public string Name { get; } = option.Name;
	public string TransedName => Tr.GetString(Name);

	public bool IsDirty { get; set; } = false;

	private readonly IReadOnlyDictionary<int, IOption> allOpt = option.Option;

	public void AddHudString(in StringBuilder builder)
	{
		builder.AppendLine($"・{Tr.GetString("OptionCategory")}: {TransedName}");

		foreach (var option in allOpt.Values)
		{
			if (!option.IsActiveAndEnable)
			{
				continue;
			}

			builder.AppendLine($"{option.TransedTitle}: {option.TransedValue}");
		}
	}

	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option)
		=> allOpt.TryGetValue(id, out option) && option is not null;
	
	public IOption Get<T>(T id) where T : Enum
		=> Get(id.FastInt());

	public T GetValue<T>(int id) where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
	{
		if (!this.TryGet(id, out var option))
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
		return option.GetValue<T>();
	}

	public IOption Get(int id)
	{
		if (!TryGet(id, out var option))
		{
			throw new ArgumentException($"OptionId: {id} Not Found");
		}
		return option;
	}
	public T GetValue<W, T>(W id) 
		where W : Enum
		where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
		=> GetValue<T>(id.FastInt());
}
