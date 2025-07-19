using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Carrier
{
    public class CarrierSpecificOption : IRoleSpecificOption
    {
        public float CarryDistance { get; set; }
        public bool CanReportOnCarry { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class CarrierOptionLoader : ISpecificOptionLoader<CarrierSpecificOption>
    {
        public CarrierSpecificOption Load(IOptionLoader loader)
        {
            return new CarrierSpecificOption
            {
                CarryDistance = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierOption.CarryDistance),
                CanReportOnCarry = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierOption.CanReportOnCarry),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class CarrierOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory, 5.0f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierOption.CarryDistance,
                1.0f, 1.0f, 5.0f, 0.5f);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Carrier.CarrierOption.CanReportOnCarry,
                true);
        }
    }
}
