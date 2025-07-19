using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Painter
{
    public class PainterSpecificOption : IRoleSpecificOption
    {
        public float CanPaintDistance { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class PainterOptionLoader : ISpecificOptionLoader<PainterSpecificOption>
    {
        public PainterSpecificOption Load(IOptionLoader loader)
        {
            return new PainterSpecificOption
            {
                CanPaintDistance = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Painter.PainterOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Painter.PainterOption.CanPaintDistance),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class PainterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Painter.PainterOption.CanPaintDistance,
                1.0f, 1.0f, 5.0f, 0.5f);
        }
    }
}
