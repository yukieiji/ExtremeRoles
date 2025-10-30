
using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class CombinedCondition(List<IOptionCondition> condition) : IOptionCondition
{
	public bool IsMet => this.condition.All(x => x.IsMet);
	public IEnumerable<IOptionCondition> Conditions => this.condition;

	private readonly List<IOptionCondition> condition = condition;

	public CombinedCondition(params IOptionCondition[] condition) : this(condition.ToList())
	{

	}
	public CombinedCondition(IEnumerable<IOptionCondition> condition) : this(condition.ToList())
	{

	}

	public void Add(IOptionCondition condition)
		=> this.condition.Add(condition);
}

public sealed class ParentActiveEnableOptionCondition(IOption option) : IOptionCondition
{
	public bool IsMet => parent.IsActiveAndEnable;
	private readonly IOption parent = option;
}

public sealed class InvertOptionCondition(IOptionCondition condition) : IOptionCondition
{
	public bool IsMet => !condition.IsMet;
	private readonly IOptionCondition condition = condition;
}

public sealed class HookOptionCondition(Func<bool> hook) : IOptionCondition
{
	public bool IsMet => hook.Invoke();
	private readonly Func<bool> hook = hook;
}

/*
public sealed class NotDefaultValueCondition(IValueHolder value) : IOptionCondition
{
	private readonly IValueHolder value = value;
	public bool IsMet => this.value.Selection != this.value.DefaultIndex;
}
*/

public sealed class AlwaysTrueCondition : IOptionCondition
{
	public bool IsMet => true;

	public static IOptionCondition operator +(AlwaysTrueCondition _, IOptionCondition other)
		=> other;
}

public static class IOptionConditionExtension
{
	public static IOptionCondition Bind(this IOptionCondition x, IOptionCondition y)
	{
		switch (x, y)
		{
			case (AlwaysTrueCondition _, AlwaysTrueCondition _):
				return x;
			case (CombinedCondition combX, CombinedCondition combY):
				return new CombinedCondition(combX.Conditions.Concat(combY.Conditions));
			case (var otherX, CombinedCondition combY):
				combY.Add(otherX);
				return combY;
			case (CombinedCondition combX, var otherY):
				combX.Add(otherY);
				return combX;
			case (var otherX, AlwaysTrueCondition _):
				return otherX;
			case (AlwaysTrueCondition _, var otherY):
				return otherY;
			default:
				return new CombinedCondition(x, y);
		}
	}
}
