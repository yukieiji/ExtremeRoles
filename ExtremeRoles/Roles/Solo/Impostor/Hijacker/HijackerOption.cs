using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Hijacker.HijackerRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Hijacker
{
    public readonly record struct HijackerSpecificOption(
        bool IsRandomPlayer,
        int AbilityUseCount,
        float AbilityActiveTime
    ) : IRoleSpecificOption;

    public class HijackerOptionLoader : ISpecificOptionLoader<HijackerSpecificOption>
    {
        public HijackerSpecificOption Load(IOptionLoader loader)
        {
            return new HijackerSpecificOption(
                loader.GetValue<Option, bool>(
                    Option.IsRandomPlayer),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime)
            );
        }
    }

    public class HijackerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 10, 10f);
            factory.CreateBoolOption(Option.IsRandomPlayer, true);
        }
    }
}
