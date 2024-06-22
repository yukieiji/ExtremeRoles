using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factories;
using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Factory;

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

	public bool IsValidRoleOption(IOptionInfo option);

    public static void CreateRoleGlobalOption()
    {
		var optMng = NewOptionManager.Instance;
		using (var roleOptionFactory = optMng.CreateColorSyncOptionCategory(
			SpawnOptionCategory.RoleSpawnCategory,
			defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}

		using (var roleOptionFactory = optMng.CreateColorSyncOptionCategory(
			SpawnOptionCategory.GhostRoleSpawnCategory,
			defaultOptionColor))
		{
			createExtremeRoleRoleSpawnOption(roleOptionFactory);
		}
		using (var xionCategory = optMng.CreateOptionCategory(
			(int)ExtremeRoleId.Xion + 200,
			Helper.Design.ColoedString(ColorPalette.XionBlue, "Xion")))
		{
			xionCategory.CreateBoolOption(
				XionOption.UseXion, false,
				color: ColorPalette.XionBlue);
		}
	}

    private static void createExtremeRoleRoleSpawnOption(ColorSyncOptionCategoryFactory factory)
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
