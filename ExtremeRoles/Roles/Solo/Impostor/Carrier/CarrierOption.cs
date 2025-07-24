using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Carrier
{
    public readonly record struct CarrierSpecificOption(
        float CarryDistance,
        bool CanReportOnCarry,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class CarrierOptionLoader : ISpecificOptionLoader<CarrierSpecificOption>
    {
        public CarrierSpecificOption Load(IOptionLoader loader)
        {
            return new CarrierSpecificOption(
                loader.GetValue<CarrierOption, float>(
                    CarrierOption.CarryDistance),
                loader.GetValue<CarrierOption, bool>(
                    CarrierOption.CanReportOnCarry),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class CarrierOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory, 5.0f);

            factory.CreateFloatOption(
                CarrierOption.CarryDistance,
                1.0f, 1.0f, 5.0f, 0.5f);

            factory.CreateBoolOption(
                CarrierOption.CanReportOnCarry,
                true);
        }
    }
}
