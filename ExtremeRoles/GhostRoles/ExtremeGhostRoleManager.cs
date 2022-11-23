using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Performance;

namespace ExtremeRoles.GhostRoles
{
    public enum ExtremeGhostRoleId : byte
    {
        VanillaRole = 0,

        Poltergeist,
        Faunus,

        Ventgeist,
        SaboEvil,

        Wisp
    }

    public enum AbilityType : byte
    {
        WispSetTorch,

        PoltergeistMoveDeadbody,
        FaunusOpenSaboConsole,

        VentgeistVentAnime,
        SaboEvilResetSabotageCool
    }

    public static class ExtremeGhostRoleManager
    {
        public const int GhostRoleOptionId = 25;

        public sealed class GhostRoleAssignData
        {
            private Dictionary<ExtremeRoleType, int> globalSpawnLimit = new Dictionary<ExtremeRoleType, int> ();

            private Dictionary<ExtremeRoleId, CombinationRoleType> CombRole = new Dictionary<
                ExtremeRoleId, CombinationRoleType>();

            // フィルター、スポーン数、スポーンレート、役職ID
            private Dictionary<ExtremeRoleType, List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>> useGhostRole = new Dictionary<
                ExtremeRoleType, List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>>();

            public GhostRoleAssignData()
            {
                this.Clear();
            }
            public void AddCombRoleAssignData(ExtremeRoleId id, CombinationRoleType type)
            {
                this.CombRole.Add(id, type);
            }

            public void Clear()
            {
                this.globalSpawnLimit.Clear();
                this.useGhostRole.Clear();
            }

            public CombinationRoleType GetCombRoleType(ExtremeRoleId roleId) => CombRole[roleId];

            public int GetGlobalSpawnLimit(ExtremeRoleType team)
            {
                if (this.globalSpawnLimit.ContainsKey(team))
                {
                    return this.globalSpawnLimit[team];
                }
                else
                {
                    return int.MinValue;
                }
            }

            public List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> GetUseGhostRole(
                ExtremeRoleType team)
            {
                if (this.useGhostRole.ContainsKey(team))
                {
                    return this.useGhostRole[team].OrderBy(
                        item => RandomGenerator.Instance.Next()).ToList();
                }
                else
                {
                    return new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> ();
                }
            }

            public bool IsCombRole(ExtremeRoleId roleId) => this.CombRole.ContainsKey(roleId);

            public bool IsGlobalSpawnLimit(ExtremeRoleType team)
            {
                try
                {
                    return this.globalSpawnLimit[team] <= 0;
                }
                catch (System.Exception e)
                {
                    ExtremeRolesPlugin.Logger.LogInfo(
                        $"Unknown teamType detect!!    tema:{team}  exception:{e}");
                    return false;
                }
            }

            public void SetGlobalSpawnLimit(int crewNum, int impNum, int neutralNum)
            {
                this.globalSpawnLimit.Add(ExtremeRoleType.Crewmate, crewNum);
                this.globalSpawnLimit.Add(ExtremeRoleType.Impostor, impNum);
                this.globalSpawnLimit.Add(ExtremeRoleType.Neutral, neutralNum);
            }
		    public void SetNormalRoleAssignData(
                ExtremeRoleType team,
                HashSet<ExtremeRoleId> filter,
                int spawnNum,
                int spawnRate,
                ExtremeGhostRoleId ghostRoleId)
            {

                (HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId) addData = (
                    filter, spawnNum, spawnRate, ghostRoleId);

                if (!this.useGhostRole.ContainsKey(team))
                {
                    List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> teamGhostRole = new List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)>()
                    {
                        addData,
                    };

                    this.useGhostRole.Add(team, teamGhostRole);
                }
                else
                {
                    this.useGhostRole[team].Add(addData);
                }
            }

            public void ReduceGlobalSpawnLimit(ExtremeRoleType team)
            {
                this.globalSpawnLimit[team] = this.globalSpawnLimit[team] - 1;
            }

            public void ReduceRoleSpawnData(
                ExtremeRoleType team,
                HashSet<ExtremeRoleId> filter,
                int spawnNum,
                int spawnRate,
                ExtremeGhostRoleId ghostRoleId)
            {
                int index = this.useGhostRole[team].FindIndex(
                    x => x == (filter, spawnNum, spawnRate, ghostRoleId));

                this.useGhostRole[team][index] = (filter, spawnNum - 1, spawnRate, ghostRoleId);

            }

        }

        public static Dictionary<byte, GhostRoleBase> GameRole = new Dictionary<byte, GhostRoleBase>();

        public static readonly Dictionary<
            ExtremeGhostRoleId, GhostRoleBase> AllGhostRole = new Dictionary<ExtremeGhostRoleId, GhostRoleBase>()
            {
                { ExtremeGhostRoleId.Poltergeist, new Poltergeist() },
                { ExtremeGhostRoleId.Faunus,      new Faunus()      },

                { ExtremeGhostRoleId.Ventgeist, new Ventgeist() },
                { ExtremeGhostRoleId.SaboEvil,  new SaboEvil()  },
            };

        private static readonly HashSet<RoleTypes> vanillaGhostRole = new HashSet<RoleTypes>()
        { 
            RoleTypes.GuardianAngel
        };

        private static GhostRoleAssignData assignData;

        public static void AddCombGhostRole(
            CombinationRoleType type,
            GhostAndAliveCombinationRoleManagerBase roleManager)
        {
            foreach (var baseRoleId in roleManager.CombGhostRole.Keys)
            {
                assignData.AddCombRoleAssignData(baseRoleId, type);
            }
        }

        public static void AssignGhostRoleToPlayer(PlayerControl player)
        {
            RoleTypes roleType = player.Data.Role.Role;

            if (vanillaGhostRole.Contains(roleType))
            {
                rpcSetSingleGhostRoleToPlayerId(
                    player, roleType,
                    ExtremeGhostRoleId.VanillaRole);
                return;
            }

            SingleRoleBase baseRole = ExtremeRoleManager.GameRole[player.PlayerId];

            ExtremeRoleType team = baseRole.Team;
            ExtremeRoleId roleId = baseRole.Id;

            if (assignData.IsGlobalSpawnLimit(team)) { return; };

            if (assignData.IsCombRole(roleId))
            {
                CombinationRoleType combRoleId = assignData.GetCombRoleType(roleId);

                // 専用のコンビ役職を取ってくる
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.SetGhostRole))
                {
                    caller.WriteBoolean(false);
                    caller.WriteByte(player.PlayerId);
                    caller.WriteByte((byte)combRoleId);
                    caller.WriteByte((byte)roleId);
                }
                setPlyaerToCombGhostRole(player.PlayerId, (byte)combRoleId, (byte)roleId);
                assignData.ReduceGlobalSpawnLimit(team);
                return;
            }

            // 各陣営の役職データを取得する
            List<(HashSet<ExtremeRoleId>, int, int, ExtremeGhostRoleId)> sameTeamRoleAssignData = assignData.GetUseGhostRole(
                team);

            foreach (var(filter, num, spawnRate, id) in sameTeamRoleAssignData)
            {
                if (filter.Count != 0 && !filter.Contains(roleId)) { continue; }
                if (!isRoleSpawn(num, spawnRate)) { continue; }
                
                rpcSetSingleGhostRoleToPlayerId(player, roleType, id);
                
                // その役職のスポーン数をへらす処理
                assignData.ReduceRoleSpawnData(
                    team, filter, num, spawnRate, id);
                // 全体の役職減少処理
                assignData.ReduceGlobalSpawnLimit(team);
     
                return;
            }
        }

        public static void CreateGhostRoleOption(int optionIdOffset)
        {
            if (AllGhostRole.Count == 0) { return; };

            IEnumerable<GhostRoleBase> roles = AllGhostRole.Values;

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
            var allOption = OptionHolder.AllOption;

            assignData.SetGlobalSpawnLimit(
                UnityEngine.Random.RandomRange(
                    allOption[(int)OptionHolder.CommonOptionKey.MinCrewmateGhostRoles].GetValue(),
                    allOption[(int)OptionHolder.CommonOptionKey.MaxCrewmateGhostRoles].GetValue()),
                UnityEngine.Random.RandomRange(
                    allOption[(int)OptionHolder.CommonOptionKey.MinImpostorGhostRoles].GetValue(),
                    allOption[(int)OptionHolder.CommonOptionKey.MaxImpostorGhostRoles].GetValue()),
                UnityEngine.Random.RandomRange(
                    allOption[(int)OptionHolder.CommonOptionKey.MinNeutralGhostRoles].GetValue(),
                    allOption[(int)OptionHolder.CommonOptionKey.MaxNeutralGhostRoles].GetValue()));

            foreach (var role in AllGhostRole.Values)
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

                assignData.SetNormalRoleAssignData(
                    role.Team,
                    role.GetRoleFilter(),
                    roleNum, spawnRate, role.Id);
            }
        }


        public static GhostRoleBase GetLocalPlayerGhostRole()
        {
            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

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

            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

            if (!GameRole.ContainsKey(playerId)) { return null; }

            var role = GameRole[playerId] as T;

            if (role != null)
            {
                return role;
            }

            return null;

        }

        public static bool IsGlobalSpawnLimit(ExtremeRoleType team) => 
            assignData.IsGlobalSpawnLimit(team);

        public static bool IsCombRole(ExtremeRoleId roleId) => assignData.IsCombRole(roleId);

        public static void Initialize()
        {
            GameRole.Clear();
            foreach (var role in AllGhostRole.Values)
            {
                role.Initialize();
            }

            if (assignData == null)
            {
                assignData = new GhostRoleAssignData();
            }
            assignData.Clear();
        }

        public static void SetGhostRoleToPlayerId(
            ref MessageReader reader)
        {
            bool isComb = reader.ReadBoolean();
            byte playerId = reader.ReadByte();
            if (isComb)
            {
                byte combType = reader.ReadByte();
                byte baseRoleId = reader.ReadByte();
                setPlyaerToCombGhostRole(playerId, combType, baseRoleId);
            }
            else
            {
                byte vanillaRoleId = reader.ReadByte();
                byte ghostRoleId = reader.ReadByte();
                setPlyaerToSingleGhostRole(playerId, vanillaRoleId, ghostRoleId);
            }
        }

        public static void UseAbility(
            byte abilityType,
            bool isReport,
            ref MessageReader reader)
        {

            AbilityType callAbility = (AbilityType)abilityType;

            switch (callAbility)
            {
                case AbilityType.VentgeistVentAnime:
                    int ventId = reader.ReadInt32();
                    Ventgeist.VentAnime(ventId);
                    break;
                case AbilityType.PoltergeistMoveDeadbody:
                    byte poltergeistPlayerId = reader.ReadByte();
                    byte poltergeistMoveDeadbodyPlayerId = reader.ReadByte();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    bool pickUp = reader.ReadBoolean();
                    Poltergeist.DeadbodyMove(
                        poltergeistPlayerId,
                        poltergeistMoveDeadbodyPlayerId,
                        x, y, pickUp);
                    break;
                case AbilityType.SaboEvilResetSabotageCool:
                    SaboEvil.ResetCool();
                    break;
                default:
                    break;
            }

            if (isReport)
            {
                ExtremeRolesPlugin.ShipState.AddGhostRoleAbilityReport(
                    callAbility);
            }
        }

        private static bool isRoleSpawn(
            int roleNum, int spawnRate)
        {
            if (roleNum <= 0) { return false; }
            if (spawnRate < UnityEngine.Random.RandomRange(0, 110)) { return false; }

            return true;
        }

        private static int computePercentage(Module.IOption self)
            => (int)System.Decimal.Multiply(self.GetValue(), self.ValueCount);

        private static void rpcSetSingleGhostRoleToPlayerId(
            PlayerControl player,
            RoleTypes baseVanillaRoleId,
            ExtremeGhostRoleId assignGhostRoleId)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.SetGhostRole))
            {
                caller.WriteBoolean(false);
                caller.WriteByte(player.PlayerId);
                caller.WriteByte((byte)baseVanillaRoleId);
                caller.WriteByte((byte)assignGhostRoleId);
            }

            setPlyaerToSingleGhostRole(
                player.PlayerId,
                (byte)baseVanillaRoleId,
                (byte)assignGhostRoleId);
        }

        private static void setPlyaerToSingleGhostRole(
            byte playerId, byte vanillaRoleId, byte roleId)
        {
            if (GameRole.ContainsKey(playerId)) { return; }

            RoleTypes roleType = (RoleTypes)vanillaRoleId;
            ExtremeGhostRoleId ghostRoleId = (ExtremeGhostRoleId)roleId;

            if (vanillaGhostRole.Contains(roleType) &&
                ghostRoleId == ExtremeGhostRoleId.VanillaRole)
            {
                lock (GameRole)
                {
                    GameRole[playerId] = new VanillaGhostRoleWrapper(roleType);
                }
                return;
            }

            GhostRoleBase role = AllGhostRole[ghostRoleId].Clone();

            role.Initialize();
            role.CreateAbility();
            lock (GameRole)
            {
                GameRole.Add(playerId, role);
            }
        }



        private static void setPlyaerToCombGhostRole(
            byte playerId, byte combType, byte baseRoleId)
        {
            if (GameRole.ContainsKey(playerId)) { return; }

            var ghostCombManager = ExtremeRoleManager.CombRole[combType] as GhostAndAliveCombinationRoleManagerBase;
            if (ghostCombManager == null) { return; }

            GhostRoleBase role = ghostCombManager.GetGhostRole((ExtremeRoleId)baseRoleId).Clone();

            role.Initialize();
            role.CreateAbility();

            lock (GameRole)
            {
                GameRole.Add(playerId, role);
            }
        }
    }
}
