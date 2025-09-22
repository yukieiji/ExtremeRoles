using ExtremeRoles.Module.CustomOption.Interfaces;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Module.CustomOption;

public sealed class OptionTabContainer(OptionTab tab)
{
	public string Name { get; } = tab.ToString();
	public int Count => this.allCategory.Count;

	public IEnumerable<IOptionCategory> Category => this.allCategory.Values;
	private readonly Dictionary<int, IOptionCategory> allCategory = new ();

	public bool TryGetCategory(int id, [NotNullWhen(true)] out IOptionCategory category)
		=> this.allCategory.TryGetValue(id, out category) && category != null;

	public void AddGroup(in IOptionCategory category)
		=> this.allCategory.Add(category.Id, category);
}
