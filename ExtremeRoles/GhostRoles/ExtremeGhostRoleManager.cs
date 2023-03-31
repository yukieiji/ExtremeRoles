using System.Collections.Generic;
using System.Linq;

using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.GhostRoles;

public enum ExtremeGhostRoleId : byte
{
    VanillaRole = 0,

    Poltergeist,
    Faunus,
    Shutter,

    Ventgeist,
    SaboEvil,
    Igniter,

    Wisp
}

public enum AbilityType : byte
{
    WispSetTorch,

    PoltergeistMoveDeadbody,
    FaunusOpenSaboConsole,
    ShutterTakePhoto,

    VentgeistVentAnime,
    SaboEvilResetSabotageCool,
    IgniterSwitchLight
}

public static class ExtremeGhostRoleManager
{
    private const int ghostRoleOptionId = 25;
    private const int idOffset = 128;

    public static Dictionary<byte, GhostRoleBase> GameRole = new Dictionary<byte, GhostRoleBase>();

    public static readonly Dictionary<
        ExtremeGhostRoleId, GhostRoleBase> AllGhostRole = new Dictionary<ExtremeGhostRoleId, GhostRoleBase>()
        {
            { ExtremeGhostRoleId.Poltergeist, new Poltergeist() },
            { ExtremeGhostRoleId.Faunus,      new Faunus()      },

            { ExtremeGhostRoleId.Ventgeist, new Ventgeist() },
            { ExtremeGhostRoleId.SaboEvil , new SaboEvil()  },
            { ExtremeGhostRoleId.Igniter  , new Igniter()   },
        };

    private static readonly HashSet<RoleTypes> vanillaGhostRole = new HashSet<RoleTypes>()
    { 
        RoleTypes.GuardianAngel,
    };

    public static void AssignGhostRoleToPlayer(PlayerControl player)
    {
        RoleTypes roleType = player.Data.Role.Role;
        SingleRoleBase baseRole = ExtremeRoleManager.GameRole[player.PlayerId];
        int controlId = baseRole.GameControlId + idOffset;

        if (vanillaGhostRole.Contains(roleType))
        {
            rpcSetSingleGhostRoleToPlayerId(
                player, controlId, roleType,
                ExtremeGhostRoleId.VanillaRole);
            return;
        }

        ExtremeRoleType team = baseRole.Team;
        ExtremeRoleId roleId = baseRole.Id;

        GhostRoleSpawnDataManager spawnDataMng = GhostRoleSpawnDataManager.Instance;

        if (spawnDataMng.IsGlobalSpawnLimit(team)) { return; };

        if (spawnDataMng.IsCombRole(roleId))
        {
            CombinationRoleType combRoleId = spawnDataMng.GetCombRoleType(roleId);

            // 専用のコンビ役職を取ってくる
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.SetGhostRole))
            {
                caller.WriteBoolean(true);
                caller.WriteByte(player.PlayerId);
                caller.WritePackedInt(controlId);
                caller.WriteByte((byte)combRoleId);
                caller.WritePackedInt((int)roleId);
            }
            setPlyaerToCombGhostRole(
                player.PlayerId, controlId, (byte)combRoleId, (int)roleId);
            spawnDataMng.ReduceGlobalSpawnLimit(team);
            return;
        }

        // 各陣営の役職データを取得する
        List<GhostRoleSpawnData> sameTeamRoleAssignData = spawnDataMng.GetUseGhostRole(team);

        if (sameTeamRoleAssignData is null) { return; }

        foreach (var spawnData in sameTeamRoleAssignData)
        {
            if (spawnData.IsBlockAliveRole(roleId) || 
                !spawnData.IsSpawn()) { continue; }
            
            rpcSetSingleGhostRoleToPlayerId(
                player, controlId, roleType, spawnData.Id);

            // その役職のスポーン数をへらす処理
            spawnData.ReduceSpawnNum();
            // 全体の役職減少処理
            spawnDataMng.ReduceGlobalSpawnLimit(team);
 
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
            roleOptionOffset = optionIdOffset + (ghostRoleOptionId * item.Index);
            item.Value.CreateRoleAllOption(roleOptionOffset);
        }

    }

    public static GhostRoleBase GetLocalPlayerGhostRole()
    {
        byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

        if (GameRole.TryGetValue(playerId, out GhostRoleBase ghostRole))
        {
            return ghostRole;
        }
        else
        {
            return null;
        }
    }
    public static T GetSafeCastedGhostRole<T>(byte playerId) where T : GhostRoleBase
    {
        if (!GameRole.TryGetValue(playerId, out GhostRoleBase ghostRole)) { return null; }

        var role = ghostRole as T;

        if (role != null)
        {
            return role;
        }

        return null;

    }

    public static T GetSafeCastedLocalPlayerRole<T>() where T : GhostRoleBase
    {

        byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

        if (!GameRole.TryGetValue(playerId, out GhostRoleBase ghostRole)) { return null; }

        var role = ghostRole as T;

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

        if (GhostRoleSpawnDataManager.IsExist)
        {
            GhostRoleSpawnDataManager.Instance.Destroy();
        }
    }

    public static void SetGhostRoleToPlayerId(
        ref MessageReader reader)
    {
        bool isComb = reader.ReadBoolean();
        
        byte playerId = reader.ReadByte();
        int controlId = reader.ReadPackedInt32();

        if (isComb)
        {
            byte combType = reader.ReadByte();
            int baseRoleId = reader.ReadPackedInt32();
            setPlyaerToCombGhostRole(
                playerId, controlId, combType, baseRoleId);
        }
        else
        {
            byte vanillaRoleId = reader.ReadByte();
            byte ghostRoleId = reader.ReadByte();
            setPlyaerToSingleGhostRole(
                playerId, controlId, vanillaRoleId, ghostRoleId);
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
            case AbilityType.WispSetTorch:
                Wisp.SetTorch(reader.ReadByte());
                break;
            case AbilityType.IgniterSwitchLight:
                Igniter.SetVison(reader.ReadBoolean());
                break;
            default:
                break;
        }

        if (isReport)
        {
            MeetingReporter.Instance.AddMeetingStartReport(
                Helper.Translation.GetString(abilityType.ToString()));
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
        => (int)decimal.Multiply(self.GetValue(), self.ValueCount);

    private static void rpcSetSingleGhostRoleToPlayerId(
        PlayerControl player,
        int gameControlId,
        RoleTypes baseVanillaRoleId,
        ExtremeGhostRoleId assignGhostRoleId)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.SetGhostRole))
        {
            caller.WriteBoolean(false);
            caller.WriteByte(player.PlayerId);
            caller.WritePackedInt(gameControlId);
            caller.WriteByte((byte)baseVanillaRoleId);
            caller.WriteByte((byte)assignGhostRoleId);
        }

        setPlyaerToSingleGhostRole(
            player.PlayerId,
            gameControlId,
            (byte)baseVanillaRoleId,
            (byte)assignGhostRoleId);
    }

    private static void setPlyaerToSingleGhostRole(
        byte playerId, int gameControlId, byte vanillaRoleId, byte roleId)
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
        
        role.SetGameControlId(gameControlId);
        role.Initialize();
        if (playerId == CachedPlayerControl.LocalPlayer.PlayerId)
        {
            role.CreateAbility();
        }
        lock (GameRole)
        {
            GameRole.Add(playerId, role);
        }
    }



    private static void setPlyaerToCombGhostRole(
        byte playerId, int gameControlId, byte combType, int baseRoleId)
    {
        if (GameRole.ContainsKey(playerId)) { return; }

        var ghostCombManager = ExtremeRoleManager.CombRole[combType] as GhostAndAliveCombinationRoleManagerBase;
        if (ghostCombManager == null) { return; }

        GhostRoleBase role = ghostCombManager.GetGhostRole((ExtremeRoleId)baseRoleId).Clone();

        role.SetGameControlId(gameControlId);
        role.Initialize();
        if (playerId == CachedPlayerControl.LocalPlayer.PlayerId)
        {
            role.CreateAbility();
        }
        ghostCombManager.InitializeGhostRole(
            playerId, role, ExtremeRoleManager.GameRole[playerId]);

        lock (GameRole)
        {
            GameRole.Add(playerId, role);
        }
    }
}
