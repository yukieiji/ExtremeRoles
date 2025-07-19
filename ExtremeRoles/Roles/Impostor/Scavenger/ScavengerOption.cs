using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Scavenger
{
    public class ScavengerSpecificOption : IRoleSpecificOption
    {
        public bool IsRandomInitAbility { get; set; }
        public bool AllowDupe { get; set; }
        public bool AllowAdvancedWeapon { get; set; }
        public int InitAbility { get; set; }
        public bool IsSetWeapon { get; set; }
        public bool SyncWeapon { get; set; }
        public int HandGunCount { get; set; }
        public float HandGunSpeed { get; set; }
        public float HandGunRange { get; set; }
        public int FlameCount { get; set; }
        public float FlameChargeTime { get; set; }
        public float FlameActiveTime { get; set; }
        public float FlameFireSecond { get; set; }
        public float FlameDeadSecond { get; set; }
        public int SwordCount { get; set; }
        public float SwordChargeTime { get; set; }
        public float SwordActiveTime { get; set; }
        public float SwordR { get; set; }
        public int SniperRifleCount { get; set; }
        public float SniperRifleSpeed { get; set; }
        public int BeamRifleCount { get; set; }
        public float BeamRifleSpeed { get; set; }
        public float BeamRifleRange { get; set; }
        public int BeamSaberCount { get; set; }
        public int BeamSaberChargeTime { get; set; }
        public float BeamSaberRange { get; set; }
        public bool BeamSaberAutoDetect { get; set; }
        public float WeaponMixTime { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class ScavengerOptionLoader : ISpecificOptionLoader<ScavengerSpecificOption>
    {
        public ScavengerSpecificOption Load(IOptionLoader loader)
        {
            return new ScavengerSpecificOption
            {
                IsRandomInitAbility = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.IsRandomInitAbility),
                AllowDupe = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.AllowDupe),
                AllowAdvancedWeapon = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.AllowAdvancedWeapon),
                InitAbility = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.InitAbility),
                IsSetWeapon = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.IsSetWeapon),
                SyncWeapon = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SyncWeapon),
                HandGunCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunCount),
                HandGunSpeed = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunSpeed),
                HandGunRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.HandGunRange),
                FlameCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameCount),
                FlameChargeTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameChargeTime),
                FlameActiveTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameActiveTime),
                FlameFireSecond = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameFireSecond),
                FlameDeadSecond = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.FlameDeadSecond),
                SwordCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordCount),
                SwordChargeTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordChargeTime),
                SwordActiveTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordActiveTime),
                SwordR = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SwordR),
                SniperRifleCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SniperRifleCount),
                SniperRifleSpeed = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.SniperRifleSpeed),
                BeamRifleCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleCount),
                BeamRifleSpeed = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleSpeed),
                BeamRifleRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamRifleRange),
                BeamSaberCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberCount),
                BeamSaberChargeTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberChargeTime),
                BeamSaberRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberRange),
                BeamSaberAutoDetect = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.BeamSaberAutoDetect),
                WeaponMixTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Scavenger.Option.WeaponMixTime),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
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
