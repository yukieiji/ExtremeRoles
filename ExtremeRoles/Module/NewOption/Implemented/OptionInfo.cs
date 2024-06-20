using System.Text.RegularExpressions;

using OptionTab = ExtremeRoles.Module.CustomOption.OptionTab;
using OptionUnit = ExtremeRoles.Module.CustomOption.OptionUnit;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.NewOption.Implemented;

public sealed class OptionInfo(
	int id, string name,
	OptionUnit format = OptionUnit.None,
	OptionTab tab = OptionTab.General,
	bool hidden = false) : IOptionInfo
{
	public int Id { get; } = id;
	public string Name { get; } = name;

	public string CodeRemovedName => nameCleaner.Replace(Name, string.Empty);

	public OptionTab Tab { get; } = tab;
	public string Format { get; } = format == OptionUnit.None ? string.Empty : format.ToString();

	public bool IsHidden { get; } = hidden;

	public override string ToString()
		=> $"Name:{Name}, Format:{Format} -- ({Id} {Tab})";

	private static readonly Regex nameCleaner = new Regex(@"(\|)|(<.*?>)|(\\n)", RegexOptions.Compiled);
}
