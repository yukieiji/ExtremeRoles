using System.Collections.Generic;

using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign
{
    public struct CombinationRoleAssignData
    {
        public byte CombType { get; private set; }
        public List<MultiAssignRoleBase> RoleList { get; private set; }
        public int GameControlId { get; private set; }

        public CombinationRoleAssignData(
            int controlId, byte combType,
            List<MultiAssignRoleBase> roleList)
        {
            CombType = combType;
            RoleList = roleList;
            GameControlId = controlId;
        }
    }

    public struct SingleRoleAssignData
    {
        public int IntedRoleId { get; private set; }
        public SingleRoleBase Role { get; private set; }

        public SingleRoleAssignData(int intedRoleId, SingleRoleBase role)
        {
            IntedRoleId = intedRoleId;
            Role = role;
        }
    }
}
