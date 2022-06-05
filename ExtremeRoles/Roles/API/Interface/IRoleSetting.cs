using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface
{
    interface IRoleOption
    {
        int GetRoleOptionId(int option);

        void Initialize();

        void CreateRoleAllOption(
            int optionIdOffset);
        void CreateRoleSpecificOption(
            CustomOptionBase parentOps,
            int optionIdOffset);
    }
}
