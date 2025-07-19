using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Impostor.SpecialImpostor
{
    public class SpecialImpostorSpecificOption : IRoleSpecificOption
    {
    }

    public class SpecialImpostorOptionLoader : ISpecificOptionLoader<SpecialImpostorSpecificOption>
    {
        public SpecialImpostorSpecificOption Load(IOptionLoader loader)
        {
            return new SpecialImpostorSpecificOption
            {
            };
        }
    }

    public class SpecialImpostorOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
        }
    }
}
