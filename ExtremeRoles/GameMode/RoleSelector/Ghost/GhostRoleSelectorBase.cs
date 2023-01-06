using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.RoleSelector.Ghost
{
    public abstract class GhostRoleSelectorBase
    {
        public void RpcAssignGhostRoleToPlayer(PlayerControl player)
        {

        }

        // 幽霊役職周り
        public abstract bool IsAssignGhostRole();
        public abstract void AssignVanillaSpecialGhostRoleWithoutNeutral();

        public abstract SingleRoleBase GetSingleRole(ExtremeRoleId id);
        public abstract CombinationRoleManagerBase GetCombinationRoleManager(CombinationRoleType id);
        public abstract List<string> GetRoleSelectorOptionList();
        public abstract void CreateRoleSelectorOptionMenu();

        protected abstract List<IAssignedPlayer> GetRoleAssignData();
    }
}
