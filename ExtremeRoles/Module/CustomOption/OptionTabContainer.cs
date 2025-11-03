using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Module.CustomOption;

public sealed class OptionTabContainer(OptionTab tab)
{
	public string Name { get; } = tab.ToString();
	public int Count => this.allCategory.Count;

	public IEnumerable<OptionCategory> Category => this.allCategory.Values;
	private readonly Dictionary<int, OptionCategory> allCategory = new ();

	public bool TryGetCategory(int id, [NotNullWhen(true)] out OptionCategory category)
		=> this.allCategory.TryGetValue(id, out category) && category != null;

	public void AddGroup(in OptionCategory category)
		=> this.allCategory.Add(category.Id, category);
}
