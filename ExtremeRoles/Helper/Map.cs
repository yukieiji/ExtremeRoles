namespace ExtremeRoles.Helper
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
