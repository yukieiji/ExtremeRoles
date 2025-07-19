using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Glitch
{
    public class GlitchSpecificOption : IRoleSpecificOption
    {
        public float Range { get; set; }
        public bool EffectOnImpo { get; set; }
        public bool EffectOnMarlin { get; set; }
        public float Delay { get; set; }
        public int ActiveTime { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class GlitchOptionLoader : ISpecificOptionLoader<GlitchSpecificOption>
    {
        public GlitchSpecificOption Load(IOptionLoader loader)
        {
            return new GlitchSpecificOption
            {
                Range = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.Range),
                EffectOnImpo = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.EffectOnImpo),
                EffectOnMarlin = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.EffectOnMarlin),
                Delay = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.Delay),
                ActiveTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Glitch.Ops.ActiveTime),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
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
