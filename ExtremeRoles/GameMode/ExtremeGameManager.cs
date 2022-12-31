using AmongUs.GameOptions;
using ExtremeRoles.GameMode.Factory;

// TODO: setプロパティ => initにする 

namespace ExtremeRoles.GameMode
{
    public class ExtremeGameManager
    {
        public static ExtremeGameManager Instance { get; private set; }

        public ShipGlobalOption ShipOption { get; private set; }

        public static void Create()
        {
            Instance = new ExtremeGameManager();

            IModeFactory factory = (GameOptionsManager.Instance.currentGameMode) switch
            {
                GameModes.Normal    => new ClassicGameModeOptionFactory(),
                GameModes.HideNSeek => new HideNSeekGameModeFactory(),
                _ => null,
            };

            Instance.ShipOption = factory.CreateGlobalOption();
        }
    }
}
