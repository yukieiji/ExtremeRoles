using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public sealed class HideNSeekGameModeFactory : IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption() => new HideNSeekModeShipGlobalOption();

        public IVisonModifier CreateVisonModifier() => new HideNSeekModeVison();
    }
}
