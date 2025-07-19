using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Magician
{
    public class MagicianSpecificOption : IRoleSpecificOption
    {
        public int TeleportTargetRate { get; set; }
        public bool DupeTeleportTargetTo { get; set; }
        public bool IncludeRolePlayer { get; set; }
        public bool IncludeSpawnPoint { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class MagicianOptionLoader : ISpecificOptionLoader<MagicianSpecificOption>
    {
        public MagicianSpecificOption Load(IOptionLoader loader)
        {
            return new MagicianSpecificOption
            {
                TeleportTargetRate = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.TeleportTargetRate),
                DupeTeleportTargetTo = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.DupeTeleportTargetTo),
                IncludeRolePlayer = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.IncludeRolePlayer),
                IncludeSpawnPoint = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Magician.MagicianOption.IncludeSpawnPoint),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
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
