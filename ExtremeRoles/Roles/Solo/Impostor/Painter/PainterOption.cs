using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.Painter.PainterRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Painter
{
    public readonly record struct PainterSpecificOption(
        float CanPaintDistance,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class PainterOptionLoader : ISpecificOptionLoader<PainterSpecificOption>
    {
        public PainterSpecificOption Load(IOptionLoader loader)
        {
            return new PainterSpecificOption(
                loader.GetValue<PainterOption, float>(
                    PainterOption.CanPaintDistance),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class PainterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory);

            factory.CreateFloatOption(
                PainterOption.CanPaintDistance,
                1.0f, 1.0f, 5.0f, 0.5f);
        }
    }
}
