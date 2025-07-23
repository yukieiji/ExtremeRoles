using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Faker
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Faker.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Faker.Option.SeeDummyMerlin),
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
                ExtremeRoles.Roles.Solo.Impostor.Faker.Option.SeeDummyMerlin,
                true);
        }
    }
}
