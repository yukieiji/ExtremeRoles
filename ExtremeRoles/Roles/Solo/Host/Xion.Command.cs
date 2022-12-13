using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        private int noXionCount = 0;

        private const char commandStarChar = '/';
        private const string noXion = "NoXion";

        private const string setRole = "SetRole";
        private const string setCombRole = "SetCombRole";
        private const string revartXion = "IamXion";

        private static Xion xionBuffer;
        private static bool voted;

        public static void ParseCommand(string chatStr)
        {
            if (chatStr[0] != commandStarChar && GameSystem.IsLobby) { return; }

            string[] args = chatStr.Substring(1).Split(" ");
            string commandBody = args[0];
            switch (commandBody)
            {
                case noXion:
                    if (!isXion())
                    {
                        noXionVoteCmd();
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
                        // 一つはコマンドボディ
                        if (args.Length == 2)
                        {
                            hostToSpecificRole(args);
                        }
                        else if (args.Length == 3)
                        {
                            specificPlayerIdToSpecificRole(args);
                        }
                        else
                        {
                            invalidArgs();
                        }
                    }
                    break;
                /* TODO:CombRoleAssign
                case setCombRole:
                    if (checkLocalOnlyCommand())
                    {

                    }
                    break;
                */
                case revartXion:
                    if (checkLocalOnlyCommand())
                    {
                        RpcHostToXion();
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
                addChat(Translation.GetString("cannotUseThisGameMode"));
                return false;
            }

        }

        private static bool isLocalGame() => 
            AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;

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

        private static void hostToSpecificRole(string[] args)
        {
            RpcRoleReplaceOps(CachedPlayerControl.LocalPlayer.PlayerId, args[1]);
        }

        private static void specificPlayerIdToSpecificRole(string[] args)
        {
            string playerName = args[1];
            playerName = playerName.Replace('_', ' ');

            byte targetPlayerId = byte.MaxValue;

            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.DefaultOutfit.PlayerName == playerName)
                {
                    targetPlayerId = player.PlayerId;
                }
            }
            if (targetPlayerId == byte.MaxValue)
            {
                addChat(Translation.GetString("invalidPlayerName"));
                return;
            }

            RpcRoleReplaceOps(targetPlayerId, args[2]);
        }

        private static void setNewRole(byte targetPlayerId, SingleRoleBase role)
        {
            lock (ExtremeRoleManager.GameRole)
            {
                ExtremeRoleManager.GameRole[targetPlayerId] = role;
            }
        }

        private static void resetRole(PlayerControl targetPlayer, byte targetPlayerId)
        {
            var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];
            IRoleHasParent.PurgeParent(targetPlayerId);

            // プレイヤーのリセット処理
            if (CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId)
            {
                abilityReset(targetRole);
            }

            // シェイプシフターのリセット処理
            shapeshiftReset(targetPlayer, targetRole);

            // スペシャルリセット処理
            specialResetRoleReset(targetPlayer, targetRole);
        }

        private static void abilityReset(
            SingleRoleBase targetRole)
        {
            var meetingResetRole = targetRole as IRoleResetMeeting;
            if (meetingResetRole != null)
            {
                meetingResetRole.ResetOnMeetingStart();
            }
            var abilityRole = targetRole as IRoleAbility;
            if (abilityRole != null)
            {
                abilityRole.ResetOnMeetingStart();
            }

            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    meetingResetRole = multiAssignRole.AnotherRole as IRoleResetMeeting;
                    if (meetingResetRole != null)
                    {
                        meetingResetRole.ResetOnMeetingStart();
                    }

                    abilityRole = multiAssignRole.AnotherRole as IRoleAbility;
                    if (abilityRole != null)
                    {
                        abilityRole.ResetOnMeetingStart();
                    }
                }
            }
        }

        private static void shapeshiftReset(
            PlayerControl targetPlayer,
            SingleRoleBase targetRole)
        {
            // シェイプシフターのリセット処理
            if (targetRole.IsVanillaRole())
            {
                if (((VanillaRoleWrapper)targetRole).VanilaRoleId == RoleTypes.Shapeshifter)
                {
                    targetPlayer.Shapeshift(targetPlayer, false);
                }
            }
        }

        private static void specialResetRoleReset(
            PlayerControl targetPlayer,
            SingleRoleBase targetRole)
        {
            var specialResetRole = targetRole as IRoleSpecialReset;
            if (specialResetRole != null)
            {
                specialResetRole.AllReset(targetPlayer);
            }

            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    specialResetRole = multiAssignRole.AnotherRole as IRoleSpecialReset;
                    if (specialResetRole != null)
                    {
                        specialResetRole.AllReset(targetPlayer);
                    }
                }
            }
        }

        private static void xionPlayerToDead(byte xionPlayerId)
        {
            GameData.PlayerInfo player = GameData.Instance.GetPlayerById(xionPlayerId);
            
            if (player.IsDead) { return; }
            
            RPCOperator.UncheckedMurderPlayer(
                xionPlayerId, xionPlayerId, byte.MaxValue);
        }

        private static void invalidArgs()
        {
            addChat(Translation.GetString("invalidArgs"));
        }

        private static void noXionVoteCmd()
        {
            if (voted)
            {
                addChat(Translation.GetString("alreadyVoted"));
            }
            else
            {
                voted = true;
                RpcNoXionVote();
            }
        }

        public void AddNoXionCount()
        {
            ++this.noXionCount;
        }
      
        private bool isNoXion() => 
            this.noXionCount >= ((GameData.Instance.AllPlayers.Count - 1) * 2 / 3);
    }
}
