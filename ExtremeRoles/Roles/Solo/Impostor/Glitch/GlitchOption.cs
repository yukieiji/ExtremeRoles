using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Glitch.GlitchRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Glitch
{
    public readonly record struct GlitchSpecificOption(
        float Range,
        bool EffectOnImpo,
        bool EffectOnMarlin,
        float Delay,
        int ActiveTime,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class GlitchOptionLoader : ISpecificOptionLoader<GlitchSpecificOption>
    {
        public GlitchSpecificOption Load(IOptionLoader loader)
        {
            return new GlitchSpecificOption(
                loader.GetValue<Ops, float>(
                    Ops.Range),
                loader.GetValue<Ops, bool>(
                    Ops.EffectOnImpo),
                loader.GetValue<Ops, bool>(
                    Ops.EffectOnMarlin),
                loader.GetValue<Ops, float>(
                    Ops.Delay),
                loader.GetValue<Ops, int>(
                    Ops.ActiveTime),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class GlitchOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 10);
            factory.CreateFloatOption(
                Ops.Range, 1.5f, 0.1f, 7.5f, 0.1f);
            var impOpt = factory.CreateBoolOption(
                Ops.EffectOnImpo, false);
            factory.CreateBoolOption(
                Ops.EffectOnMarlin, false,
                impOpt, invert: true);
            factory.CreateFloatOption(
                Ops.Delay, 5.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                Ops.ActiveTime, 10, 1, 120, 1,
                format: OptionUnit.Second);
        }
    }
}
