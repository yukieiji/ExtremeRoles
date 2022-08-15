using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion : SingleRoleBase
    {
        private int noXionCount = 0;

        private const char commandStarChar = '/';
        private const string noXion = "NoXion";

        private const string setRole = "SetRole";
        private const string setCombRole = "SetCombRole";
        private const string revartXion = "IamXion";

        public static void ParseCommand(string chatStr)
        {

            if (chatStr[0] != commandStarChar) { return; }

            string[] args = chatStr.Substring(1).Split(" ");
            string commandBody = args[0];
            switch (commandBody)
            {
                case noXion:
                    if (!isXion())
                    {
                        RpcNoXionVote();
                    }
                    break;
                default:
                    break;
            }

            // 以下シオンのみ使用できるコマンド
            if (!isXion()) { return; }

            switch (commandBody)
            {
                case setRole:
                    if (checkLocalOnlyCommand())
                    {

                    }
                    break;
                case setCombRole:
                    if (checkLocalOnlyCommand())
                    {

                    }
                    break;
                case revartXion:
                    if (checkLocalOnlyCommand())
                    {

                    }
                    break;
                default:
                    break;
            }

        }
        private static bool checkLocalOnlyCommand()
        {
            if (isLocalGame())
            { 
                return true; 
            }
            else
            {
                addChat(Helper.Translation.GetString("CannotUseThisGameMode"));
                return false;
            }

        }

        private static bool isLocalGame() => AmongUsClient.Instance.GameMode == GameModes.LocalGame;

        private static bool isXion()
        {
            return
                PlayerId != byte.MaxValue &&
                CachedPlayerControl.LocalPlayer.PlayerId == PlayerId;
        }

        private static void addChat(string text)
        {
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                CachedPlayerControl.LocalPlayer, text);
        }

        public void AddNoXionCount()
        {
            ++noXionCount;
        }
      
        private bool isNoXion() => noXionCount >= ((GameData.Instance.AllPlayers.Count - 1) * 2 / 3);
    }
}
