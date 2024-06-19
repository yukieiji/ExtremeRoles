using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionTabContainer(OptionTab tab)
{
	public string Name { get; } = tab.ToString();
	private readonly Dictionary<int, OptionGroup> allGroup = new ();

	public bool TryGetGroup(int id, [NotNullWhen(true)] out OptionGroup group)
		=> this.allGroup.TryGetValue(id, out group) && group != null;

	public void AddGroup(in OptionGroup group)
		=> this.allGroup.Add(group.Id, group);
}
