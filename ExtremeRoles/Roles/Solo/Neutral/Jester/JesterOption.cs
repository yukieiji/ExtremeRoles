using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Neutral.Jester.JesterRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Jester
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
                loader.GetValue<Option, bool>(
                    Option.UseSabotage),
                loader.GetValue<Option, float>(
                    Option.OutburstDistance),
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
                Option.OutburstDistance,
                1.0f, 0.0f, 2.0f, 0.1f);

            factory.CreateBoolOption(
                Option.UseSabotage,
                true);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 100, 2.0f);
        }
    }
}
