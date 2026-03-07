namespace ExtremeRoles.Module.CustomOption.Interfaces;

public enum MetaType
{
	Bool,
}

public interface IOptionRangeMeta
{
	public string Type { get; }
	public int Selection { get; }
	public object[] Values { get; }
}
