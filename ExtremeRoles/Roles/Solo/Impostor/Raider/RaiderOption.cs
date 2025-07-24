using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Raider.RaiderRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Raider
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
                loader.GetValue<Option, bool>(
                    Option.IsOpenLimit),
                loader.GetValue<Option, int>(
                    Option.LimitNum),
                loader.GetValue<Option, bool>(
                    Option.IsHidePlayerOnOpen),
                loader.GetValue<Option, int>(
                    Option.BombType),
                loader.GetValue<Option, int>(
                    Option.BombNum),
                loader.GetValue<Option, float>(
                    Option.BombTargetRange),
                loader.GetValue<Option, float>(
                    Option.BombRange),
                loader.GetValue<Option, float>(
                    Option.BombAliveTime),
                loader.GetValue<Option, bool>(
                    Option.BombShowOtherPlayer),
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
                Option.IsOpenLimit, true);
            factory.CreateIntOption(
                Option.LimitNum, 4, 1, 100, 1,
                limitOpt, invert: true);

            factory.CreateBoolOption(
                Option.IsHidePlayerOnOpen, true);

            var type = factory.CreateSelectionOption<Option, RaiderBombSystem.BombType>(
                Option.BombType);
            factory.CreateIntOption(Option.BombNum, 5, 2, 100, 1, type);
            factory.CreateFloatOption(Option.BombTargetRange, 1.7f, 0.1f, 25.0f, 0.1f, type);
            factory.CreateFloatOption(Option.BombRange, 1.7f, 0.1f, 5.0f, 0.1f);
            factory.CreateFloatOption(Option.BombAliveTime, 5.0f, 0.5f, 30.0f, 0.1f);

            factory.CreateBoolOption(Option.BombShowOtherPlayer, true);
        }
    }
}
