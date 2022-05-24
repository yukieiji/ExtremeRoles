using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using ExtremeRoles.GhostRoles.API;

namespace ExtremeRoles.GhostRoles
{
    public enum ExtremeGhostRoleId : byte
    {
        VanillaRole = 0,
    }

    public static class ExtremeGhostRoleManager
    {
        public const int GhostRoleOptionId = 25;

        public struct GhostRoleAssignData
        {

        }

        public static ConcurrentDictionary<byte, GhostRoleBase> GameRole = new ConcurrentDictionary<byte, GhostRoleBase>();

        public static readonly Dictionary<
            ExtremeGhostRoleId, GhostRoleBase> AllGhostRole = new Dictionary<ExtremeGhostRoleId, GhostRoleBase>()
        { 
        };

        private static readonly HashSet<RoleTypes> vanillaGhostRole = new HashSet<RoleTypes>()
        { 
            RoleTypes.GuardianAngel
        };

        private static GhostRoleAssignData assignData;

        public static void AssignGhostRoleToPlayer(PlayerControl player)
        {
            RoleTypes roleType = player.Data.Role.Role;

            if (vanillaGhostRole.Contains(roleType))
            {
                RPCOperator.Call(
                    player.NetId, RPCOperator.Command.SetGhostRole,
                    new List<byte> ()
                    {
                        player.PlayerId,
                        (byte)roleType,
                        (byte)ExtremeGhostRoleId.VanillaRole
                    });
                SetGhostRoleToPlayerId(
                    player.PlayerId, (byte)roleType,
                    (byte)ExtremeGhostRoleId.VanillaRole);
                return;
            }
        }

        public static void CreateGhostRoleOption(int optionIdOffset)
        {
            IEnumerable<GhostRoleBase> roles = AllGhostRole.Values;

            if (roles.Count() == 0) { return; };

            int roleOptionOffset = 0;

            foreach (var item in roles.Select(
                (Value, Index) => new { Value, Index }))
            {
                roleOptionOffset = optionIdOffset + (GhostRoleOptionId * item.Index);
                item.Value.CreateRoleAllOption(roleOptionOffset);
            }

        }

        public static void CreateGhostRoleAssignData()
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
            foreach (var role in AllGhostRole.Values)
            {
                role.Initialize();
            }
        }

        public static void SetGhostRoleToPlayerId(
            byte playerId, byte vanillaRoleId, byte roleId)
        {

            if (GameRole.ContainsKey(playerId)) { return; }

            RoleTypes roleType = (RoleTypes)vanillaRoleId;
            ExtremeGhostRoleId ghostRoleId = (ExtremeGhostRoleId)roleId;

            if (vanillaGhostRole.Contains(roleType) && 
                ghostRoleId == ExtremeGhostRoleId.VanillaRole)
            {
                GameRole[playerId] = new VanillaGhostRoleWrapper(roleType);
                return;
            }
            
            GhostRoleBase role = AllGhostRole[ghostRoleId].Clone();
            
            role.Initialize();
            role.CreateAbility();

            GameRole[playerId] = role;

        }
    }
}
