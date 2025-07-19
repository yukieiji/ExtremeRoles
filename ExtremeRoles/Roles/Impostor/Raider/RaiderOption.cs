using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Raider
{
    public readonly record struct RaiderSpecificOption(
        bool IsOpenLimit,
        int LimitNum,
        bool IsHidePlayerOnOpen,
        int BombType,
        int BombNum,
        float BombTargetRange,
        float BombRange,
        float BombAliveTime,
        bool BombShowOtherPlayer,
        int AbilityUseCount,
        int AbilityActiveTime
    ) : IRoleSpecificOption;

    public class RaiderOptionLoader : ISpecificOptionLoader<RaiderSpecificOption>
    {
        public RaiderSpecificOption Load(IOptionLoader loader)
        {
            return new RaiderSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.IsOpenLimit),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.LimitNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.IsHidePlayerOnOpen),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombType),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombTargetRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombAliveTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombShowOtherPlayer),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class RaiderOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 10);

            factory.CreateIntOption(
                RoleAbilityCommonOption.AbilityActiveTime,
                25, 2, 90, 1,
                format: OptionUnit.Second);

            var limitOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Raider.Option.IsOpenLimit, true);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Raider.Option.LimitNum, 4, 1, 100, 1,
                limitOpt, invert: true);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Raider.Option.IsHidePlayerOnOpen, true);

            var type = factory.CreateSelectionOption<ExtremeRoles.Roles.Solo.Impostor.Raider.Option, RaiderBombSystem.BombType>(
                ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombType);
            factory.CreateIntOption(ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombNum, 5, 2, 100, 1, type);
            factory.CreateFloatOption(ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombTargetRange, 1.7f, 0.1f, 25.0f, 0.1f, type);
            factory.CreateFloatOption(ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombRange, 1.7f, 0.1f, 5.0f, 0.1f);
            factory.CreateFloatOption(ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombAliveTime, 5.0f, 0.5f, 30.0f, 0.1f);

            factory.CreateBoolOption(ExtremeRoles.Roles.Solo.Impostor.Raider.Option.BombShowOtherPlayer, true);
        }
    }
}
