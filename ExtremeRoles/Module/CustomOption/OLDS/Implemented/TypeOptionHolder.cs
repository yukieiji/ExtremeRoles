using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.OLDS.Implemented;

public sealed class TypeOptionHolder<T> : IEnumerable<KeyValuePair<int, IValueOption<T>>>
	where T :
		struct, IComparable, IConvertible,
		IComparable<T>, IEquatable<T>
{
	private Dictionary<int, IValueOption<T>> option = new Dictionary<int, IValueOption<T>>();

	public IValueOption<T> Get(int id) => option[id];

	public bool ContainsKey(int id) => option.ContainsKey(id);

	public void Add(int id, IValueOption<T> newOption)
		=> option.Add(id, newOption);

	public void Update(int id, int selectionIndex)
	{
		lock (option)
		{
			option[id].UpdateSelection(selectionIndex);
		}
	}

	public IEnumerator<KeyValuePair<int, IValueOption<T>>> GetEnumerator() =>
		option.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() =>
		option.GetEnumerator();
}
