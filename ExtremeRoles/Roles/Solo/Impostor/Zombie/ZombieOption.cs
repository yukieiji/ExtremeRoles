using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Zombie
{
    public readonly record struct ZombieSpecificOption(
        int AwakeKillCount,
        int ResurrectKillCount,
        float ShowMagicCircleTime,
        float ResurrectDelayTime,
        bool CanResurrectOnExil,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class ZombieOptionLoader : ISpecificOptionLoader<ZombieSpecificOption>
    {
        public ZombieSpecificOption Load(IOptionLoader loader)
        {
            return new ZombieSpecificOption(
                loader.GetValue<ZombieOption, int>(
                    ZombieOption.AwakeKillCount),
                loader.GetValue<ZombieOption, int>(
                    ZombieOption.ResurrectKillCount),
                loader.GetValue<ZombieOption, float>(
                    ZombieOption.ShowMagicCircleTime),
                loader.GetValue<ZombieOption, float>(
                    ZombieOption.ResurrectDelayTime),
                loader.GetValue<ZombieOption, bool>(
                    ZombieOption.CanResurrectOnExil),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class ZombieOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ZombieOption.AwakeKillCount,
                1, 0, 3, 1,
                format: OptionUnit.Shot);

            IRoleAbility.CreateAbilityCountOption(factory, 1, 3, 3f);

            factory.CreateIntOption(
                ZombieOption.ResurrectKillCount,
                2, 0, 3, 1,
                format: OptionUnit.Shot);

            factory.CreateFloatOption(
                ZombieOption.ShowMagicCircleTime,
                10.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ZombieOption.ResurrectDelayTime,
                5.0f, 4.0f, 60.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                ZombieOption.CanResurrectOnExil,
                false);
        }
    }
}
