using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public interface IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption();

        public IRoleSelector CreateRoleSelector();

        public IVisonModifier CreateVisonModifier();
    }
}
