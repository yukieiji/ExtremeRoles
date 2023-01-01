using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.Interface
{
    internal interface IShowTextBuilder
    {
        protected const RoleCommonOption SpawnOptionKey = RoleCommonOption.SpawnRate;

        (string, string, string) GetShowText();
    }
}
