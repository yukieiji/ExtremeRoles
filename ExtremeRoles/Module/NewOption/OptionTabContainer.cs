using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionTabContainer(OptionTab tab)
{
	public string Name { get; } = tab.ToString();

	public IEnumerable<OptionCategory> Category => this.allGroup.Values;
	private readonly Dictionary<int, OptionCategory> allGroup = new ();

	public bool TryGetGroup(int id, [NotNullWhen(true)] out OptionCategory group)
		=> this.allGroup.TryGetValue(id, out group) && group != null;

	public void AddGroup(in OptionCategory group)
		=> this.allGroup.Add(group.Id, group);
}
