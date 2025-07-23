using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Crewmate.Teleporter
{
    public readonly record struct TeleporterSpecificOption(
        bool CanUseOtherPlayer,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class TeleporterOptionLoader : ISpecificOptionLoader<TeleporterSpecificOption>
    {
        public TeleporterSpecificOption Load(IOptionLoader loader)
        {
            return new TeleporterSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Teleporter.TeleporterOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Teleporter.TeleporterOption.CanUseOtherPlayer),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class TeleporterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Teleporter.TeleporterOption.CanUseOtherPlayer,
                false);
            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 3);
        }
    }
}
