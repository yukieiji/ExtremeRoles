using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.NewOption;

public sealed class OptionTabContainer(OptionTab tab)
{
	public string Name { get; } = tab.ToString();
	public int Count => this.allCategory.Count;

	public IEnumerable<OptionCategory> Category => this.allCategory.Values;
	private readonly Dictionary<int, OptionCategory> allCategory = new ();

	public bool TryGetCategory(int id, [NotNullWhen(true)] out OptionCategory group)
		=> this.allCategory.TryGetValue(id, out group) && group != null;

	public void AddGroup(in OptionCategory group)
		=> this.allCategory.Add(group.Id, group);
}
