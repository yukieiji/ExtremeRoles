using System.Collections.Generic;

using ExtremeRoles.Module;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public interface IRoleSelector
    {
        public bool CanUseXion { get; }
        public bool IsVanillaRoleToMultiAssign { get; }

        public IEnumerable<int> NormalRoleSpawnOptionId { get; }
        public IEnumerable<int> CombRoleSpawnOptionId { get; }
        public IEnumerable<int> GhostRoleSpawnOptionId { get; }

        public bool IsValidRoleOption(IOption option);
    }
}
