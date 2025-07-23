using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Scavenger
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.IsRandomInitAbility),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.AllowDupe),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.AllowAdvancedWeapon),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.InitAbility),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.IsSetWeapon),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SyncWeapon),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunSpeed),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameChargeTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameActiveTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameFireSecond),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameDeadSecond),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordChargeTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordActiveTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordR),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SniperRifleCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SniperRifleSpeed),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleSpeed),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberChargeTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberAutoDetect),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.WeaponMixTime),
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
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.IsRandomInitAbility,
                false);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.AllowDupe,
                false, randomWepon);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.AllowAdvancedWeapon,
                false, randomWepon);

            factory.CreateSelectionOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.InitAbility,
                System.Enum.GetValues<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Ability>()
                    .Select(x => x.ToString())
                    .ToArray(),
                randomWepon,
                invert: true);

            var mapSetOps = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.IsSetWeapon, true);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SyncWeapon,
                true, mapSetOps,
                invert: true);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunSpeed,
                10.0f, 0.5f, 15.0f, 0.5f);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunRange,
                3.5f, 0.1f, 5.0f, 0.1f);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameChargeTime,
                2.0f, 0.1f, 5.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameActiveTime,
                25.0f, 5.0f, 120.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameFireSecond,
                3.5f, 0.1f, 10.0f, 0.1f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameDeadSecond,
                3.5f, 0.1f, 10.0f, 0.1f,
                format: OptionUnit.Second);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordChargeTime,
                3.0f, 0.5f, 30.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordActiveTime,
                15.0f, 0.5f, 60.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordR,
                1.0f, 0.25f, 5.0f, 0.25f);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SniperRifleCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SniperRifleSpeed,
                50.0f, 25.0f, 75.0f, 0.5f);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleCount,
                1, 0, 10, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleSpeed,
                7.0f, 0.1f, 10.0f, 0.1f);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleRange,
                20.0f, 0.5f, 30.0f, 0.5f);


            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberCount,
                1, 0, 10, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberChargeTime,
                5, 1, 60, 1,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberRange,
                3.5f, 0.1f, 7.5f, 0.1f);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberAutoDetect,
                false);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.WeaponMixTime,
                3.0f, 0.5f, 25.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
