using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Neutral.Jester
{
    public readonly record struct JesterSpecificOption(
        bool UseSabotage,
        float OutburstDistance,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class JesterOptionLoader : ISpecificOptionLoader<JesterSpecificOption>
    {
        public JesterSpecificOption Load(IOptionLoader loader)
        {
            return new JesterSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Jester.JesterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Jester.JesterOption.UseSabotage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Jester.JesterOption, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Jester.JesterOption.OutburstDistance),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class JesterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.Jester.JesterOption.OutburstDistance,
                1.0f, 0.0f, 2.0f, 0.1f);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Jester.JesterOption.UseSabotage,
                true);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 100, 2.0f);
        }
    }
}
