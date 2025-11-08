using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.GameMode.RoleSelector;

public enum RoleSpawnOption : int
{
    MinCrewmate = 0,
    MaxCrewmate,
    MinNeutral,
    MaxNeutral,
    MinImpostor,
    MaxImpostor,
    MinLiberal,
    MaxLiberal,
}

public enum SpawnOptionCategory : int
{
	RoleSpawnCategory = 5,
	GhostRoleSpawnCategory,
	LiberalSetting,
}

public enum XionOption : int
{
	UseXion,
}


public interface IRoleSelector
{
    public bool IsAdjustImpostorNum { get; }

    public bool CanUseXion { get; }
    public bool IsVanillaRoleToMultiAssign { get; }

    public IEnumerable<ExtremeRoleId> UseNormalRoleId { get; }
    public IEnumerable<CombinationRoleType> UseCombRoleType { get; }
    public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId { get; }

    private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

	public bool IsValidGlobalRoleOptionId(RoleSpawnOption optionId)
		=> Enum.IsDefined(typeof(RoleSpawnOption), optionId);

	public static bool RawXionUse => OptionManager.Instance.TryGetCategory(
		OptionTab.GeneralTab,
		ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
		out var cate) &&
		cate.GetValue<bool>((int)XionOption.UseXion);

	public static bool IsCommonOption(int id)
	=> id == (int)SpawnOptionCategory.RoleSpawnCategory ||
		id == (int)SpawnOptionCategory.GhostRoleSpawnCategory ||
		(
			id > ExtremeRoleManager.RoleCategoryIdOffset &&
			id == ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion)
		);

	public bool IsValidCategory(int categoryId);

    public static void CreateRoleGlobalOption()
    {
		IOption maxLiberalSetting;
		using (var roleOptionFactory = OptionCategoryAssembler.CreateOptionCategory(
			SpawnOptionCategory.RoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
			maxLiberalSetting = createMinMaxSpawnOption(roleOptionFactory, RoleSpawnOption.MinLiberal, RoleSpawnOption.MaxLiberal, (GameSystem.VanillaMaxPlayerNum - 1) * 2);
		}

		using (var roleOptionFactory = OptionCategoryAssembler.CreateOptionCategory(
			SpawnOptionCategory.GhostRoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}

		using (var xionCategory = OptionCategoryAssembler.CreateOptionCategory(
			ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
			ExtremeRoleId.Xion.ToString(),
			color: ColorPalette.XionBlue))
		{
			xionCategory.CreateBoolOption(
				XionOption.UseXion, false);
		}

		using (var factory = OptionCategoryAssembler.CreateOptionCategory(
			SpawnOptionCategory.LiberalSetting,
			color: defaultOptionColor))
		{
			LiberalOption.Create(factory, maxLiberalSetting);
		}
	}

    private static void createExtremeRoleRoleSpawnOption(OptionCategoryFactory factory)
    {
		int maxNum = (GameSystem.VanillaMaxPlayerNum - 1) * 2;
		createMinMaxSpawnOption(factory, RoleSpawnOption.MinCrewmate, RoleSpawnOption.MaxCrewmate, maxNum);
		createMinMaxSpawnOption(factory, RoleSpawnOption.MinNeutral, RoleSpawnOption.MaxNeutral, maxNum);
		createMinMaxSpawnOption(factory, RoleSpawnOption.MinImpostor, RoleSpawnOption.MaxImpostor, GameSystem.MaxImposterNum * 2);
	}

	private static IOption createMinMaxSpawnOption(OptionCategoryFactory factory, RoleSpawnOption miniOptionEnum, RoleSpawnOption maxOptionEnum, int maxNum)
	{
		var miniOption = factory.CreateIntOption(
			miniOptionEnum,
			0, 0, maxNum, 1);

		var intRange = ValueHolderAssembler.CreateIntValue(0, 0, maxNum, 1);

		var maxOption = factory.CreateOption(maxOptionEnum, intRange);

		miniOption.OnValueChanged += () => {
			int newMini = miniOption.Value<int>();
			intRange.InnerRange = OptionRange<int>.Create(newMini, maxNum, 1);

			// Selectionを再設定
			maxOption.Selection = intRange.Selection;
		};
		return maxOption;
	}
}
