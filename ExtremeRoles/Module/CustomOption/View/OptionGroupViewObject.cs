using System.Collections.Generic;

using UnityEngine;

namespace ExtremeRoles.Module.CustomOption.View;

public sealed class OptionCategoryViewObject<T>(in CategoryHeaderMasked categoryObj, int capacity) where T : MonoBehaviour
{
	public CategoryHeaderMasked Category { get; } = categoryObj;
	public List<T> Options { get; } = new(capacity);
}
