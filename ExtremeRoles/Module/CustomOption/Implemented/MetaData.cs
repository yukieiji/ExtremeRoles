using ExtremeRoles.Module.CustomOption.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public record MetaData<T>(T[] values) : IOptionRangeMeta
{
	public string Type { get; } = typeof(T).Name;
	public object[] Values => Array.ConvertAll<T, object>(values, x =>
	{
		if (typeof(T) == typeof(string))
		{
			string s = Unsafe.As<T, string>(ref x);
			return Tr.GetString(s);
		}
		// T が値型の場合はここで「ボックス化」が発生しますが、
		// JSON化のために object[] に入れる以上、避けては通れないコストです
		return x;
	});

	private readonly T[] values = values;
}
