using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionRelation
{
	public List<IOption> Children { get; }
}

public interface IOptionParent
{
	public IOption Parent { get; }
	public bool IsChainEnable { get; }
}
