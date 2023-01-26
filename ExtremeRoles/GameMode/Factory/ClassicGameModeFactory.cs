using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public sealed class ClassicGameModeOptionFactory : IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption() => new ClassicGameModeShipGlobalOption();

        public IRoleSelector CreateRoleSelector() => new ClassicGameModeRoleSelector();

        public IVisonModifier CreateVisonModifier() => new ClassicModeVison();
    }
}
