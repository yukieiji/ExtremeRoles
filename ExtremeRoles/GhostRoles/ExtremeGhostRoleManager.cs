using System.Collections.Generic;

using ExtremeRoles.GhostRoles.API;

namespace ExtremeRoles.GhostRoles
{
    public enum ExtremeGhostRoleId
    {

    }

    public class ExtremeGhostRoleManager
    {
        public struct GhostRoleAssignData
        {

        }

        public static Dictionary<byte, GhostRoleBase> GameRole = new Dictionary<byte, GhostRoleBase>();

        public static readonly Dictionary<
            ExtremeGhostRoleId, GhostRoleBase> AllGhostRole = new Dictionary<ExtremeGhostRoleId, GhostRoleBase>();

        private static GhostRoleAssignData assignData;

        public static void AssignGhostRoleToPlayer(PlayerControl player)
        {

        }

        public static void CreateGhostRoleOption()
        {

        }

        public static GhostRoleBase GetLocalPlayerGhostRole()
        {
            byte playerId = PlayerControl.LocalPlayer.PlayerId;

            if (!GameRole.ContainsKey(playerId))
            {
                return null;
            }
            else
            {
                return GameRole[playerId];
            }
        }
        public static T GetSafeCastedGhostRole<T>(byte playerId) where T : GhostRoleBase
        {
            if (!GameRole.ContainsKey(playerId)) { return null; }

            var role = GameRole[playerId] as T;

            if (role != null)
            {
                return role;
            }

            return null;

        }

        public static T GetSafeCastedLocalPlayerRole<T>() where T : GhostRoleBase
        {

            byte playerId = PlayerControl.LocalPlayer.PlayerId;

            if (!GameRole.ContainsKey(playerId)) { return null; }

            var role = GameRole[playerId] as T;

            if (role != null)
            {
                return role;
            }

            return null;

        }

        public static void Initialize()
        {
            GameRole.Clear();
        }

        public static void SetGhostRoleAssignData(
            GhostRoleAssignData data)
        {
            assignData = data;
        }

        public static void SetGhostRoleToPlayerId(byte playerId)
        {

        }
    }
}
