using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public class HideNSeekGameModeFactory : IModeFactory
    {
        public ShipGlobalOption CreateGlobalOption() => new ShipGlobalOption();

        public IVisonModifier CreateVisonModifier() => new HideNSeekModeVison();
    }
}
