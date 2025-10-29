using System;

using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class OptionCategoryScope<T>(
	int id,
	T builder,
	Action<OptionCategory> registerFunc,
	IOptionCategoryViewInfo view) : IDisposable
	where T : IOptionBuilder
{
	public T Builder { get; } = builder;
	public IOptionCategoryViewInfo View { get; } = view;

	private readonly Action<OptionCategory> registerFunc = registerFunc;
	private readonly int id = id;

	public void Dispose()
	{
		var category = new OptionCategory(this.id, this.Builder.Option, this.View);
		registerFunc.Invoke(category);
	}
}
