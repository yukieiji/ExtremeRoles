using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Zombie
{
    public class ZombieSpecificOption : IRoleSpecificOption
    {
        public int AwakeKillCount { get; set; }
        public int ResurrectKillCount { get; set; }
        public float ShowMagicCircleTime { get; set; }
        public float ResurrectDelayTime { get; set; }
        public bool CanResurrectOnExil { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class ZombieOptionLoader : ISpecificOptionLoader<ZombieSpecificOption>
    {
        public ZombieSpecificOption Load(IOptionLoader loader)
        {
            return new ZombieSpecificOption
            {
                AwakeKillCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.AwakeKillCount),
                ResurrectKillCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.ResurrectKillCount),
                ShowMagicCircleTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.ShowMagicCircleTime),
                ResurrectDelayTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.ResurrectDelayTime),
                CanResurrectOnExil = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.CanResurrectOnExil),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class ZombieOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.AwakeKillCount,
                1, 0, 3, 1,
                format: OptionUnit.Shot);

            IRoleAbility.CreateAbilityCountOption(factory, 1, 3, 3f);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.ResurrectKillCount,
                2, 0, 3, 1,
                format: OptionUnit.Shot);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.ShowMagicCircleTime,
                10.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.ResurrectDelayTime,
                5.0f, 4.0f, 60.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Zombie.ZombieOption.CanResurrectOnExil,
                false);
        }
    }
}
