using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;


using ExtremeRoles.Module.CustomOption.Factory;

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
    IsLiberalEnabled,
    LiberalWinMoney,
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
		using (var roleOptionFactory = OptionManager.CreateOptionCategory(
			SpawnOptionCategory.RoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}

		using (var roleOptionFactory = OptionManager.CreateOptionCategory(
			SpawnOptionCategory.GhostRoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}
		using (var xionCategory = OptionManager.CreateOptionCategory(
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
		factory.CreateIntOption(
			RoleSpawnOption.MinCrewmate,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 1) * 2, 1);
		factory.CreateIntOption(
			RoleSpawnOption.MaxCrewmate,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 1) * 2, 1);

		factory.CreateIntOption(
			RoleSpawnOption.MinNeutral,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 2) * 2, 1);
		factory.CreateIntOption(
			RoleSpawnOption.MaxNeutral,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 2) * 2, 1);

		factory.CreateIntOption(
			RoleSpawnOption.MinImpostor,
			0, 0, GameSystem.MaxImposterNum * 2, 1);
		factory.CreateIntOption(
			RoleSpawnOption.MaxImpostor,
			0, 0, GameSystem.MaxImposterNum * 2, 1);

		var minLiberal = factory.CreateIntOption(
			RoleSpawnOption.MinLiberal,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 2) * 2, 1);
		var maxLiberal = factory.CreateIntOption(
			RoleSpawnOption.MaxLiberal,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 2) * 2, 1);

        var isLiberalEnabled = factory.CreateMultiDepentOption(
            RoleSpawnOption.IsLiberalEnabled,
            new() { minLiberal, maxLiberal },
            (parents) =>
            {
                var min = (ExtremeRoles.Module.CustomOption.Interfaces.IOldValueOption<int>)parents[0];
                var max = (ExtremeRoles.Module.CustomOption.Interfaces.IOldValueOption<int>)parents[1];
                return min.Value > 0 || max.Value > 0;
            });

        factory.CreateFloatOption(
            RoleSpawnOption.LiberalWinMoney,
            100f, 10f, 1000f, 10f,
            isLiberalEnabled);
    }
}
