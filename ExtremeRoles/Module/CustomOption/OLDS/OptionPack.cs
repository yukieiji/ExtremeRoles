using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.OLDS;

public sealed class OldOptionPack
{
	public IReadOnlyDictionary<int, IOldValueOption<int>> IntOptions => intOpt;
	public IReadOnlyDictionary<int, IOldValueOption<float>> FloatOptions => floatOpt;
	public IReadOnlyDictionary<int, IOldValueOption<bool>> BoolOptions => boolOpt;
	public IReadOnlyDictionary<int, IOldOption> AllOptions => allOpt;

	private readonly Dictionary<int, IOldValueOption<int>> intOpt = new Dictionary<int, IOldValueOption<int>>();
	private readonly Dictionary<int, IOldValueOption<float>> floatOpt = new Dictionary<int, IOldValueOption<float>>();
	private readonly Dictionary<int, IOldValueOption<bool>> boolOpt = new Dictionary<int, IOldValueOption<bool>>();
	private readonly Dictionary<int, IOldOption> allOpt = new Dictionary<int, IOldOption>();

	public IOldOption Get(int id) => allOpt[id];
	public IOldValueOption<T> Get<T>(int id)
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
			throw new ArgumentException("Cannot Find Options");
		}
	}

	public void Add(int id, IOldValueOption<float> option)
	{
		floatOpt.Add(id, option);
		allOpt.Add(id, option);
	}
	public void Add(int id, IOldValueOption<int> option)
	{
		intOpt.Add(id, option);
		allOpt.Add(id, option);
	}
	public void Add(int id, IOldValueOption<bool> option)
	{
		boolOpt.Add(id, option);
		allOpt.Add(id, option);
	}

	public void AddOption<SelectionType>(int id, IOldValueOption<SelectionType> option)
		where SelectionType :
			struct, IComparable, IConvertible,
			IComparable<SelectionType>, IEquatable<SelectionType>
	{
		if (typeof(SelectionType) == typeof(int))
		{
			Add(id, Unsafe.As<IOldValueOption<SelectionType>, IOldValueOption<int>>(ref option));
		}
		else if (typeof(SelectionType) == typeof(float))
		{
			Add(id, Unsafe.As<IOldValueOption<SelectionType>, IOldValueOption<float>>(ref option));
		}
		else if (typeof(SelectionType) == typeof(bool))
		{
			Add(id, Unsafe.As<IOldValueOption<SelectionType>, IOldValueOption<bool>>(ref option));
		}
		else
		{
			throw new ArgumentException("Cannot Add Options");
		}
	}
}
