using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ExtremeRoles.Module.NewOption;

using ExtremeRoles.Module.CustomOption;

public sealed class OptionPack
{
	public IReadOnlyDictionary<int, IValueOption<int>> IntOptions => intOpt;
	public IReadOnlyDictionary<int, IValueOption<float>> FloatOptions => floatOpt;
	public IReadOnlyDictionary<int, IValueOption<bool>> BoolOptions => boolOpt;
	public IReadOnlyDictionary<int, IOptionInfo> AllOptions => allOpt;

	private readonly Dictionary<int, IValueOption<int>> intOpt = new Dictionary<int, IValueOption<int>>();
	private readonly Dictionary<int, IValueOption<float>> floatOpt = new Dictionary<int, IValueOption<float>>();
	private readonly Dictionary<int, IValueOption<bool>> boolOpt = new Dictionary<int, IValueOption<bool>>();
	private readonly Dictionary<int, IOptionInfo> allOpt = new Dictionary<int, IOptionInfo>();

	public void Add(int id, IValueOption<float> option)
	{
		this.floatOpt.Add(id, option);
		this.allOpt.Add(id, option);
	}
	public void Add(int id, IValueOption<int> option)
	{
		this.intOpt.Add(id, option);
		this.allOpt.Add(id, option);
	}
	public void Add(int id, IValueOption<bool> option)
	{
		this.boolOpt.Add(id, option);
		this.allOpt.Add(id, option);
	}

	public void AddOption<SelectionType>(int id, IValueOption<SelectionType> option)
		where SelectionType :
			struct, IComparable, IConvertible,
			IComparable<SelectionType>, IEquatable<SelectionType>
	{
		if (typeof(SelectionType) == typeof(int))
		{
			Add(id, Unsafe.As<IValueOption<SelectionType>, IValueOption<int>>(ref option));
		}
		else if (typeof(SelectionType) == typeof(float))
		{
			Add(id, Unsafe.As<IValueOption<SelectionType>, IValueOption<float>>(ref option));
		}
		else if (typeof(SelectionType) == typeof(bool))
		{
			Add(id, Unsafe.As<IValueOption<SelectionType>, IValueOption<bool>>(ref option));
		}
		else
		{
			throw new ArgumentException("Cannot Add Options");
		}
	}
}
