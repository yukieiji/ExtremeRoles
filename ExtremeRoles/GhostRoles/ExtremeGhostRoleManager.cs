using AmongUs.GameOptions;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.GhostRoles.Impostor;
using ExtremeRoles.GhostRoles.Neutal;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Hazel;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeRoles.GhostRoles;

public enum ExtremeGhostRoleId : byte
{
    VanillaRole = 0,

    Wisp,

    Poltergeist,
    Faunus,
    Shutter,

    Ventgeist,
    SaboEvil,
    Igniter,
	Doppelganger,

	Foras,
}

public enum AbilityType : byte
{
    WispSetTorch,

    PoltergeistMoveDeadbody,
    FaunusOpenSaboConsole,
    ShutterTakePhoto,

    VentgeistVentAnime,
    SaboEvilResetSabotageCool,
    IgniterSwitchLight,
	DoppelgangerDoppel,

	ForasShowArrow
}

public static class ExtremeGhostRoleManager
{
    public const int IdOffset = 512;

    public static Dictionary<byte, GhostRoleBase> GameRole = new Dictionary<byte, GhostRoleBase>();

    public static readonly Dictionary<
        ExtremeGhostRoleId, GhostRoleBase> AllGhostRole = new Dictionary<ExtremeGhostRoleId, GhostRoleBase>()
        {
            { ExtremeGhostRoleId.Poltergeist, new Poltergeist() },
            { ExtremeGhostRoleId.Faunus,      new Faunus()      },
            { ExtremeGhostRoleId.Shutter,     new Shutter()     },

            { ExtremeGhostRoleId.Ventgeist   , new Ventgeist()    },
            { ExtremeGhostRoleId.SaboEvil    , new SaboEvil()     },
            { ExtremeGhostRoleId.Igniter     , new Igniter()      },
			{ ExtremeGhostRoleId.Doppelganger, new Doppelganger() },

			{ ExtremeGhostRoleId.Foras    , new Foras()   },
        };

    private static readonly HashSet<RoleTypes> vanillaGhostRole = new HashSet<RoleTypes>()
    {
        RoleTypes.GuardianAngel,
    };

	private const int roleIdOffset = 512;

	public static int GetRoleGroupId(ExtremeGhostRoleId roleId)
		=> roleIdOffset + (int)roleId;

	public static void AssignGhostRoleToPlayer(PlayerControl player)
    {
        RoleTypes roleType = player.Data.Role.Role;
        SingleRoleBase baseRole = ExtremeRoleManager.GameRole[player.PlayerId];
        int controlId = baseRole.GameControlId + IdOffset;

        if (vanillaGhostRole.Contains(roleType))
        {
            rpcSetSingleGhostRoleToPlayerId(
                player, controlId, roleType,
                ExtremeGhostRoleId.VanillaRole);
            return;
        }

        ExtremeRoleType team = baseRole.Core.Team;
        ExtremeRoleId roleId = baseRole.Core.Id;

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
            var ghostRoleId = spawnData.Id;
            if (!spawnData.IsFilterContain(baseRole) ||
                !spawnData.IsSpawn() ||
                RoleAssignFilter.Instance.IsBlock(ghostRoleId)) { continue; }

            rpcSetSingleGhostRoleToPlayerId(
                player, controlId, roleType, ghostRoleId);

            // その役職のスポーン数をへらす処理
            spawnData.ReduceSpawnNum();
            // 全体の役職減少処理
            spawnDataMng.ReduceGlobalSpawnLimit(team);

            RoleAssignFilter.Instance.Update(ghostRoleId);

            return;
        }
    }

    public static void CreateGhostRoleOption(AutoRoleOptionCategoryFactory factory)
    {
        foreach (var ghost in AllGhostRole.Values)
		{
			ghost.CreateRoleAllOption(factory);
		}
    }

    public static GhostRoleBase GetLocalPlayerGhostRole()
    {
        byte playerId = PlayerControl.LocalPlayer.PlayerId;

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

        byte playerId = PlayerControl.LocalPlayer.PlayerId;

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
            case AbilityType.IgniterSwitchLight:
                Igniter.SetVison(reader.ReadBoolean());
                break;
			case AbilityType.DoppelgangerDoppel:
				byte doppelgangerPlayerId = reader.ReadByte();
				byte doppelTargetPlayerId = reader.ReadByte();
				Doppelganger.Doppl(doppelgangerPlayerId, doppelTargetPlayerId);
				break;
			case AbilityType.ForasShowArrow:
                Foras.SwitchArrow(ref reader);
                break;
			default:
                break;
        }

        if (isReport)
        {
            MeetingReporter.Instance.AddMeetingStartReport(
                Tr.GetString(callAbility.ToString()));
        }
    }

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
        if (playerId == PlayerControl.LocalPlayer.PlayerId)
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
        if (playerId == PlayerControl.LocalPlayer.PlayerId)
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
