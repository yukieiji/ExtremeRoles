using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public interface IModeFactory
    {
        public IShipGlobalOption CreateGlobalOption();

        public IVisonModifier CreateVisonModifier();
    }
}
