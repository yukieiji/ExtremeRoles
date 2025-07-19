using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Slime
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Slime.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Slime.Option.SeeMorphMerlin),
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
                ExtremeRoles.Roles.Solo.Impostor.Slime.Option.SeeMorphMerlin, false);
        }
    }
}
