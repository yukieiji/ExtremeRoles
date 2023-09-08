﻿using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Module.CustomOption.Factorys;

namespace ExtremeRoles.GameMode.RoleSelector;

public enum RoleGlobalOption : int
{
    MinCrewmateRoles = 10,
    MaxCrewmateRoles,
    MinNeutralRoles,
    MaxNeutralRoles,
    MinImpostorRoles,
    MaxImpostorRoles,

    MinCrewmateGhostRoles,
    MaxCrewmateGhostRoles,
    MinNeutralGhostRoles,
    MaxNeutralGhostRoles,
    MinImpostorGhostRoles,
    MaxImpostorGhostRoles,

    UseXion,
}

public interface IRoleSelector
{
    public bool IsAdjustImpostorNum { get; }

    public bool CanUseXion { get; }
    public bool EnableXion { get; }
    public bool IsVanillaRoleToMultiAssign { get; }

    public IEnumerable<ExtremeRoleId> UseNormalRoleId { get; }
    public IEnumerable<CombinationRoleType> UseCombRoleType { get; }
    public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId { get; }

    private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

    public void Load();

    public bool IsCanUseAndEnableXion() => CanUseXion && EnableXion;

    public bool IsValidGlobalRoleOptionId(RoleGlobalOption optionId);

    public bool IsValidRoleOption(IOptionInfo option);

    public static void CreateRoleGlobalOption()
    {
		var roleOptionFactory = new ColorSyncFactory(defaultOptionColor);

        createExtremeRoleGlobalSpawnOption(roleOptionFactory);
        createExtremeGhostRoleGlobalSpawnOption(roleOptionFactory);

		roleOptionFactory.CreateBoolOption(
			RoleGlobalOption.UseXion, false,
			isHeader:true, color: ColorPalette.XionBlue);
    }

    private static void createExtremeRoleGlobalSpawnOption(ColorSyncFactory factory)
    {
		factory.CreateIntOption(
			RoleGlobalOption.MinCrewmateRoles,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 1) * 2, 1,
			isHeader: true);
		factory.CreateIntOption(
			RoleGlobalOption.MaxCrewmateRoles,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 1) * 2, 1);

		factory.CreateIntOption(
			RoleGlobalOption.MinNeutralRoles,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 2) * 2, 1);
		factory.CreateIntOption(
			RoleGlobalOption.MaxNeutralRoles,
			0, 0, (GameSystem.VanillaMaxPlayerNum - 2) * 2, 1);

		factory.CreateIntOption(
			RoleGlobalOption.MinImpostorRoles,
			0, 0, GameSystem.MaxImposterNum * 2, 1);
		factory.CreateIntOption(
			RoleGlobalOption.MaxImpostorRoles,
			0, 0, GameSystem.MaxImposterNum * 2, 1);
    }

    private static void createExtremeGhostRoleGlobalSpawnOption(ColorSyncFactory factory)
	{
		factory.CreateIntOption(
			RoleGlobalOption.MinCrewmateGhostRoles,
			0, 0, GameSystem.VanillaMaxPlayerNum - 1, 1,
			isHeader: true);
		factory.CreateIntOption(
			RoleGlobalOption.MaxCrewmateGhostRoles,
			0, 0, GameSystem.VanillaMaxPlayerNum - 1, 1);

		factory.CreateIntOption(
			RoleGlobalOption.MinNeutralGhostRoles,
			0, 0, GameSystem.VanillaMaxPlayerNum - 2, 1);
		factory.CreateIntOption(
			RoleGlobalOption.MaxNeutralGhostRoles,
			0, 0, GameSystem.VanillaMaxPlayerNum - 2, 1);

		factory.CreateIntOption(
			RoleGlobalOption.MinImpostorGhostRoles,
			0, 0, GameSystem.MaxImposterNum, 1);
		factory.CreateIntOption(
			RoleGlobalOption.MaxImpostorGhostRoles,
			0, 0, GameSystem.MaxImposterNum, 1);
    }
}
