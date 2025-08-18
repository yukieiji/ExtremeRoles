using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class AlwaysTrueCondition : IOptionCondition
{
	public bool IsMet => true;
}

public sealed class NotDefaultValueCondition(IValueHolder value) : IOptionCondition
{
	private readonly IValueHolder value = value;
	public bool IsMet => this.value.Selection != this.value.DefaultIndex;
}
