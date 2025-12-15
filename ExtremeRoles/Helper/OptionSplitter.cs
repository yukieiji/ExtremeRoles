using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.OLDS;

#nullable enable

namespace ExtremeRoles.Helper;

public static class OptionSplitter
{
	public static HashSet<int> AllEnable => [];

	public static bool TryGetValidOption(in OptionCategory cate, [NotNullWhen(true)] out IReadOnlySet<int>? optionId)
	{
		int id = cate.Id;

		optionId = default;

		var instance = ExtremeGameModeManager.Instance;

		return
		(
			(
				cate.Tab is OptionTab.GeneralTab &&
				(
					OptionCreator.IsCommonOption(id) ||
					IRoleSelector.IsCommonOption(id) ||
					instance.ShipOption.TryGetInvalidOption(id, out optionId)
				)
			) ||
			instance.RoleSelector.IsValidCategory(id)
		);
	}
	public static bool IsValidOption(IReadOnlySet<int>? optionId, int id)
		=> optionId is null || optionId.Count == 0 || optionId.Contains(id);
}
