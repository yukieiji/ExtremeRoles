using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionCategory(
	int id,
	string name,
	in OptionPack option)
{
	public IEnumerable<IOptionInfo> AllOption => allOpt.Values;
	public int Count => allOpt.Count;

	public int Id { get; } = id;
	public string Name { get; } = name;

	private readonly ImmutableDictionary<int, IValueOption<int>> intOpt = option.IntOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IValueOption<float>> floatOpt = option.FloatOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IValueOption<bool>> boolOpt = option.BoolOptions.ToImmutableDictionary();
	private readonly ImmutableDictionary<int, IOptionInfo> allOpt = option.AllOptions.ToImmutableDictionary();

	public T GetValue<T>(int id)
	{
		if (typeof(T) == typeof(int))
		{
			var intOption = this.intOpt[id];
			int intValue = intOption.GetValue();
			return Unsafe.As<int, T>(ref intValue);
		}
		else if (typeof(T) == typeof(float))
		{
			var floatOption = this.floatOpt[id];
			float floatValue = floatOption.GetValue();
			return Unsafe.As<float, T>(ref floatValue);
		}
		else if (typeof(T) == typeof(bool))
		{
			var boolOption = this.boolOpt[id];
			bool boolValue = boolOption.GetValue();
			return Unsafe.As<bool, T>(ref boolValue);
		}
		else
		{
			return default(T);
		}
	}
}
