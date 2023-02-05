using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;

namespace ExtremeRoles.GameMode.RoleSelector
{
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
        public bool CanUseXion { get; }
        public bool IsVanillaRoleToMultiAssign { get; }

        public IEnumerable<ExtremeRoleId> UseNormalRoleId { get; }
        public IEnumerable<CombinationRoleType> UseCombRoleType { get; }
        public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId { get; }

        private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

        public bool IsValidRoleOption(IOption option);

        public static void CreateRoleGlobalOption()
        {
            createExtremeRoleGlobalSpawnOption();
            createExtremeGhostRoleGlobalSpawnOption();

            new BoolCustomOption(
                (int)RoleGlobalOption.UseXion,
                Design.ColoedString(
                    ColorPalette.XionBlue,
                    RoleGlobalOption.UseXion.ToString()),
                false, null, true);
        }

        private static void createExtremeRoleGlobalSpawnOption()
        {
            new IntCustomOption(
                (int)RoleGlobalOption.MinCrewmateRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MinCrewmateRoles.ToString()),
                0, 0, (OptionHolder.VanillaMaxPlayerNum - 1) * 2, 1, null, true);
            new IntCustomOption(
                (int)RoleGlobalOption.MaxCrewmateRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MaxCrewmateRoles.ToString()),
                0, 0, (OptionHolder.VanillaMaxPlayerNum - 1) * 2, 1);

            new IntCustomOption(
                (int)RoleGlobalOption.MinNeutralRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MinNeutralRoles.ToString()),
                0, 0, (OptionHolder.VanillaMaxPlayerNum - 2) * 2, 1);
            new IntCustomOption(
                (int)RoleGlobalOption.MaxNeutralRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MaxNeutralRoles.ToString()),
                0, 0, (OptionHolder.VanillaMaxPlayerNum - 2) * 2, 1);

            new IntCustomOption(
                (int)RoleGlobalOption.MinImpostorRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MinImpostorRoles.ToString()),
                0, 0, OptionHolder.MaxImposterNum * 2, 1);
            new IntCustomOption(
                (int)RoleGlobalOption.MaxImpostorRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MaxImpostorRoles.ToString()),
                0, 0, OptionHolder.MaxImposterNum * 2, 1);
        }

        private static void createExtremeGhostRoleGlobalSpawnOption()
        {
            new IntCustomOption(
                (int)RoleGlobalOption.MinCrewmateGhostRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MinCrewmateGhostRoles.ToString()),
                0, 0, OptionHolder.VanillaMaxPlayerNum - 1, 1, null, true);
            new IntCustomOption(
                (int)RoleGlobalOption.MaxCrewmateGhostRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MaxCrewmateGhostRoles.ToString()),
                0, 0, OptionHolder.VanillaMaxPlayerNum - 1, 1);

            new IntCustomOption(
                (int)RoleGlobalOption.MinNeutralGhostRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MinNeutralGhostRoles.ToString()),
                0, 0, OptionHolder.VanillaMaxPlayerNum - 2, 1);
            new IntCustomOption(
                (int)RoleGlobalOption.MaxNeutralGhostRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MaxNeutralGhostRoles.ToString()),
                0, 0, OptionHolder.VanillaMaxPlayerNum - 2, 1);

            new IntCustomOption(
                (int)RoleGlobalOption.MinImpostorGhostRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MinImpostorGhostRoles.ToString()),
                0, 0, OptionHolder.MaxImposterNum, 1);
            new IntCustomOption(
                (int)RoleGlobalOption.MaxImpostorGhostRoles,
                Design.ColoedString(
                    defaultOptionColor,
                    RoleGlobalOption.MaxImpostorGhostRoles.ToString()),
                0, 0, OptionHolder.MaxImposterNum, 1);
        }

    }
}
