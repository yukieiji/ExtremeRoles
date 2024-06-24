﻿using System;
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

	public static bool RawXionUse => NewOptionManager.Instance.TryGetCategory(
		OptionTab.General,
		ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
		out var cate) &&
		cate.GetValue<bool>((int)XionOption.UseXion);

	public bool IsValidCategory(int categoryId);

    public static void CreateRoleGlobalOption()
    {
		using (var roleOptionFactory = NewOptionManager.CreateOptionCategory(
			SpawnOptionCategory.RoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}

		using (var roleOptionFactory = NewOptionManager.CreateOptionCategory(
			SpawnOptionCategory.GhostRoleSpawnCategory,
			color: defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}
		using (var xionCategory = NewOptionManager.CreateOptionCategory(
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
    }
}
