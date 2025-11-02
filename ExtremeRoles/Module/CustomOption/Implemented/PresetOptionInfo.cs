using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class PresetOptionInfo(int id, string name) : IOptionInfo
{
	public int Id { get; } = id;

	public string Name { get; } = name;

	public string CodeRemovedName { get; } = name;

	public string Format { get; } = OptionUnit.Preset.ToString();

	public bool IsHidden { get; } = false;
}
