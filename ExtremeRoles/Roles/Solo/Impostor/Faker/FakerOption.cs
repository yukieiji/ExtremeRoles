using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Faker.FakerRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Faker
{
    public readonly record struct FakerSpecificOption(
        bool SeeDummyMerlin,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class FakerOptionLoader : ISpecificOptionLoader<FakerSpecificOption>
    {
        public FakerSpecificOption Load(IOptionLoader loader)
        {
            return new FakerSpecificOption(
                loader.GetValue<Option, bool>(
                    Option.SeeDummyMerlin),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class FakerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory);
            factory.CreateBoolOption(
                Option.SeeDummyMerlin,
                true);
        }
    }
}
