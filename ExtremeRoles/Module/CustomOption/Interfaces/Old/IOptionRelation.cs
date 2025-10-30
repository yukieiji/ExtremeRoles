using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption.Interfaces.Old;

public interface IOptionRelation
{
	public List<IOption> Children { get; }
}

public interface IOptionChain
{
	public bool IsChainEnable { get; }
}

public interface IOptionParent
{
	public IOption Parent { get; }
}

public interface IOptionInvertRelation
{ }
