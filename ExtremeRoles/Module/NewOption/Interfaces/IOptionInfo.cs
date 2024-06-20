using ExtremeRoles.GhostRoles.Neutal;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.NewOption.Interfaces;

public interface IOptionInfo
{
	public int Id { get; }
	public string Name { get; }

	public string CodeRemovedName { get; }

	public OptionTab Tab { get; }
	public OptionUnit Format { get; }

	public bool IsHidden { get; }

	public string ToString();
}
