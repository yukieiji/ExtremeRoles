using System.Collections.Generic;
using ExtremeRoles.Core.Abstract.CustomOption;

namespace ExtremeRoles.Core.Infrastructure.CustomOption;

public sealed class OptionPack
{
	public IReadOnlyDictionary<int, IOption> AllOptions => allOpt;

	private readonly Dictionary<int, IOption> allOpt = new Dictionary<int, IOption>();

	public IOption Get(int id) => this.allOpt[id];

	public void AddOption(int id, IOption option)
	{
		this.allOpt.Add(id, option);
	}
}
