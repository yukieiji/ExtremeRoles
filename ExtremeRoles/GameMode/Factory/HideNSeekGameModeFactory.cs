using ExtremeRoles.GameMode.Logic.Usable;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles.GameMode.Factory
{
    public sealed class HideNSeekGameModeFactory : IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption() => new HideNSeekModeShipGlobalOption();

        public IRoleSelector CreateRoleSelector() => new HideNSeekGameModeRoleSelector();

        public ILogicUsable CreateLogicUsable() => new HideNSeekModeUsableLogic();
    }
}
