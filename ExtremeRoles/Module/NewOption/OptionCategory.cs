using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using ExtremeRoles.Module.NewOption.Interfaces;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionCategory(
	int id,
	string name,
	in OptionPack option)
{
	public IEnumerable<IOption> Options => allOpt.Values;
	public int Count => allOpt.Count;

	public int Id { get; } = id;
	public string Name { get; } = name;
	public bool IsDirty { get; set; } = false;

	private readonly ImmutableDictionary<int, IValueOption<int>> intOpt = option.IntOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IValueOption<float>> floatOpt = option.FloatOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IValueOption<bool>> boolOpt = option.BoolOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IOption> allOpt = option.AllOptions.ToImmutableDictionary();

	public bool TryGet(int id, out IOption option)
		=> this.allOpt.TryGetValue(id, out option) && option is not null;

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
