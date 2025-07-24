using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Crewmate.Teleporter.TeleporterRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Teleporter
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
                loader.GetValue<TeleporterOption, bool>(
                    TeleporterOption.CanUseOtherPlayer),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class TeleporterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                TeleporterOption.CanUseOtherPlayer,
                false);
            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 3);
        }
    }
}
