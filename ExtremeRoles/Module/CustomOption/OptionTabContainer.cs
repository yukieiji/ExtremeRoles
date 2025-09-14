using ExtremeRoles.Module.CustomOption.OLDS;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Module.CustomOption;

public sealed class OptionTabContainer(OptionTab tab)
{
	public string Name { get; } = tab.ToString();
	public int Count => this.allCategory.Count;

	public IEnumerable<OldOptionCategory> Category => this.allCategory.Values;
	private readonly Dictionary<int, OldOptionCategory> allCategory = new ();

	public bool TryGetCategory(int id, [NotNullWhen(true)] out OldOptionCategory category)
		=> this.allCategory.TryGetValue(id, out category) && category != null;

	public void AddGroup(in OldOptionCategory category)
		=> this.allCategory.Add(category.Id, category);
}
