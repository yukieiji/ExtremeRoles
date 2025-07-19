using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Host.Xion
{
    public class XionSpecificOption : IRoleSpecificOption
    {
    }

    public class XionOptionLoader : ISpecificOptionLoader<XionSpecificOption>
    {
        public XionSpecificOption Load(IOptionLoader loader)
        {
            return new XionSpecificOption
            {
            };
        }
    }

    public class XionOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
        }
    }
}
