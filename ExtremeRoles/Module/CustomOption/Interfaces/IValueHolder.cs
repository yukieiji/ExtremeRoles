namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IValueHolder
{
	public int DefaultIndex { get; }
	public string StrValue { get; }
	public int Selection { get; set; }
	public int Range { get; }
}
