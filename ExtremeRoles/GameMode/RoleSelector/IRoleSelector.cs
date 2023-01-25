using System;
using System.Collections.Generic;
using System.Text;

using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public interface IRoleSelector
    {
        public bool CanUseXion { get; }

        public IReadOnlyList<int> UseNormalRoleOptionId { get; }
        public IReadOnlyList<int> UseCombRoleOptionId { get; }
        public IReadOnlyList<int> UseGhostRoleOptionId { get; }

        // public SingleRoleBase GetVanilaRole(RoleTypes roleId);
        public bool IsValidRoleOption(IOption option);
    }
}
