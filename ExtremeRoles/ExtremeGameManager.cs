using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Factory;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Vison;

// TODO: setプロパティ => initにする 

namespace ExtremeRoles
{
    public class ExtremeGameManager
    {
        public const int SameNeutralGameControlId = int.MaxValue;

        public static ExtremeGameManager Instance { get; private set; }

        public IShipGlobalOption ShipOption { get; private set; }

        public IVisonModifier Vison { get; private set; }

        public static void Create(GameModes mode)
        {
            Instance = new ExtremeGameManager();

            IModeFactory factory = mode switch
            {
                GameModes.Normal => new ClassicGameModeOptionFactory(),
                GameModes.HideNSeek => new HideNSeekGameModeFactory(),
                _ => null,
            };

            Instance.ShipOption = factory.CreateGlobalOption();
            Instance.Vison = factory.CreateVisonModifier();
        }

        public void Load()
        {
            Instance.ShipOption.Load();
        }
    }
}
