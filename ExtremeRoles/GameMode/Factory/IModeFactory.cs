using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public interface IModeFactory
    {
        public ShipGlobalOption CreateGlobalOption();

        public IVisonModifier CreateVisonModifier();
    }
}
