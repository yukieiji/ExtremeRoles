using ExtremeRoles.GameMode.Option;

// TODO: setプロパティ => initにする 

namespace ExtremeRoles.GameMode
{
    public class ExtremeGameManager
    {
        public static ExtremeGameManager Instance { get; private set; }

        public ShipGlobalOption ShipOption { get; set; }

        public static void Create()
        {

        }
    }
}
