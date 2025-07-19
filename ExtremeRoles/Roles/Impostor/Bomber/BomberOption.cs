using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Bomber
{
    public class BomberSpecificOption : IRoleSpecificOption
    {
        public int ExplosionRange { get; set; }
        public int ExplosionKillChance { get; set; }
        public float TimerMaxTime { get; set; }
        public float TimerMinTime { get; set; }
        public bool TellExplosion { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
    }

    public class BomberOptionLoader : ISpecificOptionLoader<BomberSpecificOption>
    {
        public BomberSpecificOption Load(IOptionLoader loader)
        {
            return new BomberSpecificOption
            {
                ExplosionRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionRange),
                ExplosionKillChance = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionKillChance),
                TimerMaxTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMaxTime),
                TimerMinTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMinTime),
                TellExplosion = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TellExplosion),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            };
        }
    }

    public class BomberOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5, 2.5f);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionRange,
                2, 1, 5, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.ExplosionKillChance,
                50, 25, 75, 1,
                format: OptionUnit.Percentage);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMinTime,
                15f, 5.0f, 30f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TimerMaxTime,
                60f, 45f, 75f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Bomber.BomberOption.TellExplosion,
                true);
        }
    }
}
