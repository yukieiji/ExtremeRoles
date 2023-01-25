using System;
using System.Collections.Generic;

using ExtremeRoles.Module;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public interface IRoleSelector
    {
        public bool CanUseXion { get; }
        public bool IsVanillaRoleToMultiAssign { get; }

        public IReadOnlyList<int> UseNormalRoleOptionId { get; }
        public IReadOnlyList<int> UseCombRoleOptionId { get; }
        public IReadOnlyList<int> UseGhostRoleOptionId { get; }

        public bool IsValidRoleOption(IOption option);
    }
}
