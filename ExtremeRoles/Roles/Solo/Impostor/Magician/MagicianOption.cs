using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Magician
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.TeleportTargetRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.DupeTeleportTargetTo),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.IncludeRolePlayer),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.IncludeSpawnPoint),
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
                ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.TeleportTargetRate,
                100, 10, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.DupeTeleportTargetTo,
                true);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.IncludeSpawnPoint,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.IncludeRolePlayer,
                false);
        }
    }
}
