using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles.GameMode.Factory
{
    public sealed class ClassicGameModeFactory : IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption() => new ClassicGameModeShipGlobalOption();

        public IRoleSelector CreateRoleSelector() => new ClassicGameModeRoleSelector();
    }
}
