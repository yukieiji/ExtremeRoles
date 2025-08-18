using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption.Interfaces;

public interface IOptionRelation
{
	public List<IOldOption> Children { get; }
}

public interface IOptionChain
{
	public bool IsChainEnable { get; }
}

public interface IOptionParent
{
	public IOldOption Parent { get; }
}

public interface IOptionInvertRelation
{ }
