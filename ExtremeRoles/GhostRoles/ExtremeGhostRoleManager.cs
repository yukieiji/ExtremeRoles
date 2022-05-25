using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
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

        public class GhostRoleAssignData
        {
            public int CrewNum;
            public int ImpostorNum;
            public int NeutralNum;

            public Dictionary<ExtremeRoleId, CombinationRoleType> CombRole;

            // フィルター、スポーン数、スポーンレート、役職ID
            public List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> UseImpostorRole;
            public List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> UseCrewGhostRole;
            public List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> UseNeutralGhostRole;
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
                rpcSetGhostRoleToPlayerId(
                    player, roleType,
                    ExtremeGhostRoleId.VanillaRole);
                return;
            }

            var baseRole = ExtremeRoleManager.GameRole[player.PlayerId];
            // 全体の役職数チェック

            // コンビ役職のチェック


            // 各陣営の役職データを取得する
            List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> sameTeamRoleAssignData = new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>();

            foreach (var(filter, num, spawnRate, id) in sameTeamRoleAssignData)
            {
                if (filter.Count != 0 && !filter.Contains(baseRole.Id)) { continue; }
                if (isRoleSpawn(num, spawnRate)) { continue; }

                // 全体の役職減少処理

                rpcSetGhostRoleToPlayerId(player, roleType, id);
                
                // その役職のスポーン数をへらす処理
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

        public static void CreateGhostRoleAssignData(
            Dictionary<ExtremeRoleId, CombinationRoleType> useGhostCombRole)
        {
            var allOption = OptionHolder.AllOption;


            List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> crewGhostRole = 
                new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> ();
            List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> impGhostRole =
                new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>();
            List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> neutralGhostRole =
                new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>();


            foreach (var (roleId, role) in AllGhostRole)
            {
                int spawnRate = computePercentage(allOption[
                    role.GetRoleOptionId(RoleCommonOption.SpawnRate)]);
                int roleNum = allOption[
                    role.GetRoleOptionId(RoleCommonOption.RoleNum)].GetValue();

                Helper.Logging.Debug(
                    $"GhostRole Name:{role.Name}  SpawnRate:{spawnRate}   RoleNum:{roleNum}");

                if (roleNum <= 0 || spawnRate <= 0.0)
                {
                    continue;
                }

                (HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId) addData = 
                    (role.GetRoleFilter(), roleNum, spawnRate, role.Id);

                switch (role.Team)
                {
                    case ExtremeRoleType.Crewmate:
                        crewGhostRole.Add(addData);
                        break;
                    case ExtremeRoleType.Impostor:
                        impGhostRole.Add(addData);
                        break;
                    case ExtremeRoleType.Neutral:
                        neutralGhostRole.Add(addData);
                        break;
                    case ExtremeRoleType.Null:
                        break;
                    default:
                        throw new System.Exception("Unknown teamType detect!!");
                }
            }

            assignData = new GhostRoleAssignData
            {
                CrewNum = UnityEngine.Random.RandomRange(
                    allOption[(int)OptionHolder.CommonOptionKey.MinCrewmateGhostRoles].GetValue(),
                    allOption[(int)OptionHolder.CommonOptionKey.MaxCrewmateGhostRoles].GetValue()),
                NeutralNum = UnityEngine.Random.RandomRange(
                    allOption[(int)OptionHolder.CommonOptionKey.MinNeutralGhostRoles].GetValue(),
                    allOption[(int)OptionHolder.CommonOptionKey.MaxNeutralGhostRoles].GetValue()),
                ImpostorNum = UnityEngine.Random.RandomRange(
                    allOption[(int)OptionHolder.CommonOptionKey.MinImpostorGhostRoles].GetValue(),
                    allOption[(int)OptionHolder.CommonOptionKey.MaxImpostorGhostRoles].GetValue()),

                CombRole = useGhostCombRole,
                UseCrewGhostRole = crewGhostRole,
                UseImpostorRole = impGhostRole,
                UseNeutralGhostRole = neutralGhostRole,
            };
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

        private static bool isRoleSpawn(
            int roleNum, int spawnRate)
        {
            if (roleNum <= 0) { return false; }
            if (spawnRate < UnityEngine.Random.RandomRange(0, 110)) { return false; }

            return true;
        }

        private static int computePercentage(Module.CustomOptionBase self)
            => (int)System.Decimal.Multiply(self.GetValue(), self.Selections.ToList().Count);


        private static void rpcSetGhostRoleToPlayerId(
            PlayerControl player,
            RoleTypes baseVanillaRoleId,
            ExtremeGhostRoleId assignGhostRoleId)
        {
            RPCOperator.Call(
                player.NetId, RPCOperator.Command.SetGhostRole,
                new List<byte>()
                {
                    player.PlayerId,
                    (byte)baseVanillaRoleId,
                    (byte)assignGhostRoleId
                });
            SetGhostRoleToPlayerId(
                player.PlayerId,
                (byte)baseVanillaRoleId,
                (byte)assignGhostRoleId);
        }
    }
}
