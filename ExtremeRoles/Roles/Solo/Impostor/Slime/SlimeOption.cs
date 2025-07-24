using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Slime.SlimeRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Slime
{
    public readonly record struct SlimeSpecificOption(
        bool SeeMorphMerlin,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class SlimeOptionLoader : ISpecificOptionLoader<SlimeSpecificOption>
    {
        public SlimeSpecificOption Load(IOptionLoader loader)
        {
            return new SlimeSpecificOption(
                loader.GetValue<Option, bool>(
                    Option.SeeMorphMerlin),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class SlimeOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory, 30.0f);
            factory.CreateBoolOption(
                Option.SeeMorphMerlin, false);
        }
    }
}
