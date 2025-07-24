using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Scavenger.ScavengerRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Scavenger
{
    public readonly record struct ScavengerSpecificOption(
        bool IsRandomInitAbility,
        bool AllowDupe,
        bool AllowAdvancedWeapon,
        int InitAbility,
        bool IsSetWeapon,
        bool SyncWeapon,
        int HandGunCount,
        float HandGunSpeed,
        float HandGunRange,
        int FlameCount,
        float FlameChargeTime,
        float FlameActiveTime,
        float FlameFireSecond,
        float FlameDeadSecond,
        int SwordCount,
        float SwordChargeTime,
        float SwordActiveTime,
        float SwordR,
        int SniperRifleCount,
        float SniperRifleSpeed,
        int BeamRifleCount,
        float BeamRifleSpeed,
        float BeamRifleRange,
        int BeamSaberCount,
        int BeamSaberChargeTime,
        float BeamSaberRange,
        bool BeamSaberAutoDetect,
        float WeaponMixTime,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class ScavengerOptionLoader : ISpecificOptionLoader<ScavengerSpecificOption>
    {
        public ScavengerSpecificOption Load(IOptionLoader loader)
        {
            return new ScavengerSpecificOption(
                loader.GetValue<Option, bool>(
                    Option.IsRandomInitAbility),
                loader.GetValue<Option, bool>(
                    Option.AllowDupe),
                loader.GetValue<Option, bool>(
                    Option.AllowAdvancedWeapon),
                loader.GetValue<Option, int>(
                    Option.InitAbility),
                loader.GetValue<Option, bool>(
                    Option.IsSetWeapon),
                loader.GetValue<Option, bool>(
                    Option.SyncWeapon),
                loader.GetValue<Option, int>(
                    Option.HandGunCount),
                loader.GetValue<Option, float>(
                    Option.HandGunSpeed),
                loader.GetValue<Option, float>(
                    Option.HandGunRange),
                loader.GetValue<Option, int>(
                    Option.FlameCount),
                loader.GetValue<Option, float>(
                    Option.FlameChargeTime),
                loader.GetValue<Option, float>(
                    Option.FlameActiveTime),
                loader.GetValue<Option, float>(
                    Option.FlameFireSecond),
                loader.GetValue<Option, float>(
                    Option.FlameDeadSecond),
                loader.GetValue<Option, int>(
                    Option.SwordCount),
                loader.GetValue<Option, float>(
                    Option.SwordChargeTime),
                loader.GetValue<Option, float>(
                    Option.SwordActiveTime),
                loader.GetValue<Option, float>(
                    Option.SwordR),
                loader.GetValue<Option, int>(
                    Option.SniperRifleCount),
                loader.GetValue<Option, float>(
                    Option.SniperRifleSpeed),
                loader.GetValue<Option, int>(
                    Option.BeamRifleCount),
                loader.GetValue<Option, float>(
                    Option.BeamRifleSpeed),
                loader.GetValue<Option, float>(
                    Option.BeamRifleRange),
                loader.GetValue<Option, int>(
                    Option.BeamSaberCount),
                loader.GetValue<Option, int>(
                    Option.BeamSaberChargeTime),
                loader.GetValue<Option, float>(
                    Option.BeamSaberRange),
                loader.GetValue<Option, bool>(
                    Option.BeamSaberAutoDetect),
                loader.GetValue<Option, float>(
                    Option.WeaponMixTime),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class ScavengerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                API.Interface.IRoleAbility.DefaultCoolTime,
                API.Interface.IRoleAbility.MinCoolTime,
                API.Interface.IRoleAbility.MaxCoolTime,
                API.Interface.IRoleAbility.Step,
                format: OptionUnit.Second);

            var randomWepon = factory.CreateBoolOption(
                Option.IsRandomInitAbility,
                false);

            factory.CreateBoolOption(
                Option.AllowDupe,
                false, randomWepon);
            factory.CreateBoolOption(
                Option.AllowAdvancedWeapon,
                false, randomWepon);

            factory.CreateSelectionOption(
                Option.InitAbility,
                System.Enum.GetValues<Ability>()
                    .Select(x => x.ToString())
                    .ToArray(),
                randomWepon,
                invert: true);

            var mapSetOps = factory.CreateBoolOption(
                Option.IsSetWeapon, true);

            factory.CreateBoolOption(
                Option.SyncWeapon,
                true, mapSetOps,
                invert: true);

            factory.CreateIntOption(
                Option.HandGunCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                Option.HandGunSpeed,
                10.0f, 0.5f, 15.0f, 0.5f);
            factory.CreateFloatOption(
                Option.HandGunRange,
                3.5f, 0.1f, 5.0f, 0.1f);

            factory.CreateIntOption(
                Option.FlameCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                Option.FlameChargeTime,
                2.0f, 0.1f, 5.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.FlameActiveTime,
                25.0f, 5.0f, 120.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.FlameFireSecond,
                3.5f, 0.1f, 10.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.FlameDeadSecond,
                3.5f, 0.1f, 10.0f, 0.1f,
                format: OptionUnit.Second);

            factory.CreateIntOption(
                Option.SwordCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                Option.SwordChargeTime,
                3.0f, 0.5f, 30.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.SwordActiveTime,
                15.0f, 0.5f, 60.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.SwordR,
                1.0f, 0.25f, 5.0f, 0.25f);

            factory.CreateIntOption(
                Option.SniperRifleCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                Option.SniperRifleSpeed,
                50.0f, 25.0f, 75.0f, 0.5f);

            factory.CreateIntOption(
                Option.BeamRifleCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                Option.BeamRifleSpeed,
                7.0f, 0.1f, 10.0f, 0.1f);
            factory.CreateFloatOption(
                Option.BeamRifleRange,
                20.0f, 0.5f, 30.0f, 0.5f);


            factory.CreateIntOption(
                Option.BeamSaberCount,
                1, 0, 10, 1);
            factory.CreateIntOption(
                Option.BeamSaberChargeTime,
                5, 1, 60, 1,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                Option.BeamSaberRange,
                3.5f, 0.1f, 7.5f, 0.1f);
            factory.CreateBoolOption(
                Option.BeamSaberAutoDetect,
                false);

            factory.CreateFloatOption(
                Option.WeaponMixTime,
                3.0f, 0.5f, 25.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
