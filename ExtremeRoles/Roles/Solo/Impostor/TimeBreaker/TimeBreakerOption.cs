using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.TimeBreakerRole;

namespace ExtremeRoles.Roles.Solo.Impostor.TimeBreaker
{
    public readonly record struct TimeBreakerSpecificOption(
        float ActiveTime,
        bool EffectImp,
        bool EffectMarlin,
        bool IsActiveScreen,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class TimeBreakerOptionLoader : ISpecificOptionLoader<TimeBreakerSpecificOption>
    {
        public TimeBreakerSpecificOption Load(IOptionLoader loader)
        {
            return new TimeBreakerSpecificOption(
                loader.GetValue<Opt, float>(
                    Opt.ActiveTime),
                loader.GetValue<Opt, bool>(
                    Opt.EffectImp),
                loader.GetValue<Opt, bool>(
                    Opt.EffectMarlin),
                loader.GetValue<Opt, bool>(
                    Opt.IsActiveScreen),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class TimeBreakerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 100);
            factory.CreateFloatOption(
                Opt.ActiveTime, 10.0f, 1.0f, 120.0f, 0.5f,
                format: OptionUnit.Second);
            var impOpt = factory.CreateBoolOption(
                Opt.EffectImp, true);
            factory.CreateBoolOption(
                Opt.EffectMarlin, false,
                impOpt);
            factory.CreateBoolOption(
                Opt.IsActiveScreen, true);
        }
    }
}
