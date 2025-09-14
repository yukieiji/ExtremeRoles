using AmongUs.GameOptions;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.GhostRoles.Crewmate;
using ExtremeRoles.GhostRoles.Crewmate.Faunus;
using ExtremeRoles.GhostRoles.Crewmate.Poltergeist;
using ExtremeRoles.GhostRoles.Crewmate.Shutter;
using ExtremeRoles.GhostRoles.Impostor.Doppelganger;
using ExtremeRoles.GhostRoles.Impostor.Igniter;
using ExtremeRoles.GhostRoles.Impostor.SaboEvil;
using ExtremeRoles.GhostRoles.Impostor.Ventgeist;
using ExtremeRoles.GhostRoles.Neutral.Foras;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using Hazel;
using Microsoft.Extensions.DependencyInjection;
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

	public static void RegisterService(IServiceCollection service)
	{
		service
			.AddTransient<IGhostRoleCoreProvider, GhostRoleCoreProvider>()
			.AddTransient<IGhostRoleOptionBuilderProvider, GhostRoleOptionBuilderProvider>()
			.AddTransient<IGhostRoleOptionBuildManager, GhostRoleOptionBuildManager>();

		service
			.AddTransient<PoltergeistOptionBuilder>()
			.AddTransient<FaunusOptionBuilder>()
			.AddTransient<ShutterOptionBuilder>()

			.AddTransient<VentgeistOptionBuilder>()
			.AddTransient<SaboEvilOptionBuilder>()
			.AddTransient<IgniterOptionBuilder>()
			.AddTransient<DoppelgangerOptionBuilder>()
			
			.AddTransient<ForasOptionBuilder>();

		service
			.AddSingleton<IGhostRoleInfoContainer, GhostRoleInfo>()
			.AddSingleton<IGhostRoleProvider, GhostRoleProvider>();
	}

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

    public static void CreateGhostRoleOption()
    {
		var builder = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGhostRoleOptionBuildManager>();
		builder.Build();
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
                VentgeistRole.VentAnime(ventId);
                break;
            case AbilityType.PoltergeistMoveDeadbody:
                byte poltergeistPlayerId = reader.ReadByte();
                byte poltergeistMoveDeadbodyPlayerId = reader.ReadByte();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                bool pickUp = reader.ReadBoolean();
                PoltergeistRole.DeadbodyMove(
                    poltergeistPlayerId,
                    poltergeistMoveDeadbodyPlayerId,
                    x, y, pickUp);
                break;
            case AbilityType.SaboEvilResetSabotageCool:
                SaboEvilRole.ResetCool();
                break;
            case AbilityType.IgniterSwitchLight:
                IgniterRole.SetVison(reader.ReadBoolean());
                break;
			case AbilityType.DoppelgangerDoppel:
				byte doppelgangerPlayerId = reader.ReadByte();
				byte doppelTargetPlayerId = reader.ReadByte();
				DoppelgangerRole.Doppl(doppelgangerPlayerId, doppelTargetPlayerId);
				break;
			case AbilityType.ForasShowArrow:
                ForasRole.SwitchArrow(ref reader);
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

		var provider = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGhostRoleProvider>();
		GhostRoleBase role = provider.Get(ghostRoleId);

		role.SetGameControlId(gameControlId);
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
        (role as Wisp).Initialize();
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
