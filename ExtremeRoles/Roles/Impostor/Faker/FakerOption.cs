using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Faker
{
    public class FakerSpecificOption : IRoleSpecificOption
    {
        public bool SeeDummyMerlin { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class FakerOptionLoader : ISpecificOptionLoader<FakerSpecificOption>
    {
        public FakerSpecificOption Load(IOptionLoader loader)
        {
            return new FakerSpecificOption
            {
                SeeDummyMerlin = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Faker.Option, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Faker.Option.SeeDummyMerlin),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
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
