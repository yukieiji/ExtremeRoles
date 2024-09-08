using System.Collections.Generic;

using UnityEngine;

namespace ExtremeRoles.Module.CustomOption.View;

public sealed class OptionCategoryViewObject<T>(OptionCategoryViewObject<T>.Builder builder) where T : MonoBehaviour
{
	public class Builder(in CategoryHeaderMasked categoryObj, int capacity)
	{
		public CategoryHeaderMasked Category { get; } = categoryObj;
		public List<T> Options { get; } = new(capacity);

		public OptionCategoryViewObject<T> Build() => new OptionCategoryViewObject<T>(this);
	}

	public CategoryHeaderMasked Category { get; } = builder.Category;
	public T[] View { get; } = builder.Options.ToArray();
}
