
namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionInfo
{
	public int Id { get; }
	public string Name { get; }

	public string CodeRemovedName { get; }

	public string Format { get; }

	public bool IsHidden { get; }

	public string ToString();
}
