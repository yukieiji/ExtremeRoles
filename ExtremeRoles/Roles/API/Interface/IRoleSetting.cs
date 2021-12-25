using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface
{
    interface IRoleSetting
    {
        int GetRoleSettingId(int setting);

        void GameInit();

        void CreateRoleAllOption(
            int optionIdOffset);
        void CreatRoleSpecificOption(
            CustomOption parentOps,
            int optionIdOffset);
    }
}
