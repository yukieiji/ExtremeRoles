using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Magician
{
    public readonly record struct MagicianSpecificOption(
        int TeleportTargetRate,
        bool DupeTeleportTargetTo,
        bool IncludeRolePlayer,
        bool IncludeSpawnPoint,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class MagicianOptionLoader : ISpecificOptionLoader<MagicianSpecificOption>
    {
        public MagicianSpecificOption Load(IOptionLoader loader)
        {
            return new MagicianSpecificOption(
                loader.GetValue<MagicianOption, int>(
                    MagicianOption.TeleportTargetRate),
                loader.GetValue<MagicianOption, bool>(
                    MagicianOption.DupeTeleportTargetTo),
                loader.GetValue<MagicianOption, bool>(
                    MagicianOption.IncludeRolePlayer),
                loader.GetValue<MagicianOption, bool>(
                    MagicianOption.IncludeSpawnPoint),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class MagicianOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(factory, 1, 10);

            factory.CreateIntOption(
                MagicianOption.TeleportTargetRate,
                100, 10, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                MagicianOption.DupeTeleportTargetTo,
                true);
            factory.CreateBoolOption(
                MagicianOption.IncludeSpawnPoint,
                false);
            factory.CreateBoolOption(
                MagicianOption.IncludeRolePlayer,
                false);
        }
    }
}
