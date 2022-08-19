using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

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

        private static Xion xionBuffer; 

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
                        rpcHostToXion();
                    }
                    break;
                default:
                    break;
            }

        }

        public static void HostToXion(byte hostPlayerId)
        {
            xionPlayerToDead(hostPlayerId);
            resetRole(hostPlayerId);
            setNewRole(hostPlayerId, xionBuffer);
            xionBuffer = null;
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

        private static void rpcHostToXion()
        {
            if (xionBuffer == null)
            {
                addChat(Helper.Translation.GetString("XionNow"));
                return;
            }

            addChat(Helper.Translation.GetString("RevartXionStart"));

            byte xionPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.ReplaceRole,
                new List<byte>
                {
                    xionPlayerId,
                    xionPlayerId,
                    (byte)ExtremeRoleManager.ReplaceOperation.CreateServant
                });
            HostToXion(xionPlayerId);

            addChat(Helper.Translation.GetString("RevartXionEnd"));
        }

        private static void setNewRole(byte targetPlayerId, SingleRoleBase role)
        {
            lock (ExtremeRoleManager.GameRole)
            {
                ExtremeRoleManager.GameRole[targetPlayerId] = role;
            }
        }

        private static void resetRole(byte targetPlayerId)
        {
            var targetPlayer = Helper.Player.GetPlayerControlById(targetPlayerId);
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

        public void AddNoXionCount()
        {
            ++this.noXionCount;
        }
      
        private bool isNoXion() => this.noXionCount >= ((GameData.Instance.AllPlayers.Count - 1) * 2 / 3);
    }
}
