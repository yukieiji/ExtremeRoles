using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.TimeBreaker
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt, float>(
                    ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.ActiveTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.EffectImp),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.EffectMarlin),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.IsActiveScreen),
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
                ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.ActiveTime, 10.0f, 1.0f, 120.0f, 0.5f,
                format: OptionUnit.Second);
            var impOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.EffectImp, true);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.EffectMarlin, false,
                impOpt);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.TimeBreaker.Opt.IsActiveScreen, true);
        }
    }
}
