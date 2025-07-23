using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Glitch
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.Range),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.EffectOnImpo),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.EffectOnMarlin),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.Delay),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.ActiveTime),
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
                ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.Range, 1.5f, 0.1f, 7.5f, 0.1f);
            var impOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.EffectOnImpo, false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.EffectOnMarlin, false,
                impOpt, invert: true);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.Delay, 5.0f, 0.0f, 30.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.ActiveTime, 10, 1, 120, 1,
                format: OptionUnit.Second);
        }
    }
}
