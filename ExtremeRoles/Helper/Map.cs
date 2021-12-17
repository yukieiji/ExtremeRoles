namespace ExtremeRoles.Modules.Helpers
{
    public class Map
    {

        public static bool IsGameLobby
        {
            get
            {
                return (
                    AmongUsClient.Instance.GameState !=
                    InnerNet.InnerNetClient.GameStates.Started
                );
            }
        }
    }
}
