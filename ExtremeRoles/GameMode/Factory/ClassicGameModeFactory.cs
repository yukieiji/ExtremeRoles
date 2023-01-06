using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public sealed class ClassicGameModeOptionFactory : IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption()
        {
            return new ClassicGameModeShipGlobalOption();
        }

        public IVisonModifier CreateVisonModifier() => new ClassicModeVison();
    }
}
