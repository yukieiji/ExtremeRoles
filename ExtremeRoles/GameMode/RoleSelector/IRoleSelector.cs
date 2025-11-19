using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.OLDS;
using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles.GameMode.RoleSelector;

public enum RoleSpawnOption : int
{
    MinCrewmate = 0,
    MaxCrewmate,
    MinNeutral,
    MaxNeutral,
    MinImpostor,
    MaxImpostor,
}

public enum SpawnOptionCategory : int
{
	RoleSpawnCategory = 5,
	GhostRoleSpawnCategory,
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
		using (var roleOptionFactory = OptionCategoryAssembler.CreateOptionCategory(
			SpawnOptionCategory.RoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
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
	}

    private static void createExtremeRoleRoleSpawnOption(OptionCategoryFactory factory)
    {
		createMinMaxSpawnOption(factory, RoleSpawnOption.MinCrewmate, RoleSpawnOption.MaxCrewmate, (GameSystem.VanillaMaxPlayerNum - 1) * 2);
		createMinMaxSpawnOption(factory, RoleSpawnOption.MinNeutral, RoleSpawnOption.MaxNeutral, (GameSystem.VanillaMaxPlayerNum - 2) * 2);
		createMinMaxSpawnOption(factory, RoleSpawnOption.MinImpostor, RoleSpawnOption.MaxImpostor, GameSystem.MaxImposterNum * 2);
    }

	private static void createMinMaxSpawnOption(OptionCategoryFactory factory, RoleSpawnOption miniOptionEnum, RoleSpawnOption maxOptionEnum, int maxNum)
	{
		var miniOption = factory.CreateIntOption(
			miniOptionEnum,
			0, 0, maxNum, 1);

		var intRange = ValueHolderAssembler.CreateIntValue(0, 0, maxNum, 1);

		factory.CreateOption(maxOptionEnum, intRange);

		miniOption.OnValueChanged += () => {
			int newMini = miniOption.Value<int>();
			intRange.InnerRange = OptionRange<int>.Create(newMini, maxNum, 1);
		};
	}
}
