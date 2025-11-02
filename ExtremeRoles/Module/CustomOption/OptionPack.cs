using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;

namespace ExtremeRoles.Module.CustomOption;

public sealed class OptionPack
{
	public IReadOnlyDictionary<int, IValueOption<int>> IntOptions => intOpt;
	public IReadOnlyDictionary<int, IValueOption<float>> FloatOptions => floatOpt;
	public IReadOnlyDictionary<int, IValueOption<bool>> BoolOptions => boolOpt;
	public IReadOnlyDictionary<int, IOldOption> AllOptions => allOpt;

	private readonly Dictionary<int, IValueOption<int>> intOpt = new Dictionary<int, IValueOption<int>>();
	private readonly Dictionary<int, IValueOption<float>> floatOpt = new Dictionary<int, IValueOption<float>>();
	private readonly Dictionary<int, IValueOption<bool>> boolOpt = new Dictionary<int, IValueOption<bool>>();
	private readonly Dictionary<int, IOldOption> allOpt = new Dictionary<int, IOldOption>();

	public IOldOption Get(int id) => this.allOpt[id];
	public IValueOption<T> Get<T>(int id)
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
			throw new ArgumentException("Cannot Find Options");
		}
	}

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
