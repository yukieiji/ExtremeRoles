using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface
{
    interface IRoleOption
    {
        int GetRoleOptionId(int Option);

        void GameInit();

        void CreateRoleAllOption(
            int optionIdOffset);
        void CreatRoleSpecificOption(
            CustomOptionBase parentOps,
            int optionIdOffset);
    }
}
