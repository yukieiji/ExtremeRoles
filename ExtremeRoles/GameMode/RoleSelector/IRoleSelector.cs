using System.Collections.Generic;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public interface IRoleSelector
    {
        public bool CanUseXion { get; }
        public bool IsVanillaRoleToMultiAssign { get; }

        public IEnumerable<ExtremeRoleId> UseNormalRoleId { get; }
        public IEnumerable<CombinationRoleType> UseCombRoleType { get; }
        public IEnumerable<ExtremeGhostRoleId> UseGhostRoleId { get; }

        public bool IsValidRoleOption(IOption option);
    }
}
