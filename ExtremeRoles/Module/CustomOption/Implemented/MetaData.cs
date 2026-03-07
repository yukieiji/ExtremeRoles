using ExtremeRoles.Module.CustomOption.Interfaces;
using System.Linq;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public record MetaData<T>(int selection, T[] values) : IOptionRangeMeta
{
	public string Type { get; } = typeof(T).Name;
	public int Selection { get; } = selection;
	public object[] Values => values.Select(x => (object)x).ToArray();


	private readonly T[] values = values;
}
