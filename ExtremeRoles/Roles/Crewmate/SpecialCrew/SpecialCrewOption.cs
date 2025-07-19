using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Crewmate.SpecialCrew
{
    public class SpecialCrewSpecificOption : IRoleSpecificOption
    {
    }

    public class SpecialCrewOptionLoader : ISpecificOptionLoader<SpecialCrewSpecificOption>
    {
        public SpecialCrewSpecificOption Load(IOptionLoader loader)
        {
            return new SpecialCrewSpecificOption
            {
            };
        }
    }

    public class SpecialCrewOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
        }
    }
}
