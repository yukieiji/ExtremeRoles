using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using AmongUs.GameOptions;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Host;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles;

#nullable enable

public enum ExtremeRoleId : int
{
    Null = -100,
    VanillaRole = 50,

    Assassin,
    Marlin,
    Lover,
    Supporter,
    Hero,
    Villain,
    Vigilante,
    Detective,
    Assistant,
    DetectiveApprentice,
    Sharer,
    Guesser,
    Delinquent,
    Buddy,
    Mover,
	Accelerator,
	Skater,

	SpecialCrew,
    Sheriff,
    Maintainer,
    Neet,
    Watchdog,
    Supervisor,
    BodyGuard,
    Whisper,
    TimeMaster,
    Agency,
    Bakary,
    CurseMaker,
    Fencer,
    Opener,
    Carpenter,
    Survivor,
    Captain,
    Photographer,
    Delusioner,
    Resurrecter,
    Gambler,
    Teleporter,
	Moderator,
	Psychic,
	Bait,
	Jailer,
	Yardbird,
	Summoner,

	SpecialImpostor,
    Evolver,
    Carrier,
    PsychoKiller,
    BountyHunter,
    Painter,
    Faker,
    OverLoader,
    Cracker,
    Bomber,
    Mery,
    SlaveDriver,
    SandWorm,
    Smasher,
    AssaultMaster,
    Shooter,
    LastWolf,
    Commander,
    Hypnotist,
    UnderWarper,
    Magician,
    Slime,
    Zombie,
	Thief,
	Crewshroom,
	Terorist,
	Raider,
	Glitch,
	Hijacker,
	TimeBreaker,

	Alice,
    Jackal,
    Sidekick,
    TaskMaster,
    Missionary,
    Jester,
    Yandere,
    Yoko,
    Totocalcio,
    Miner,
    Eater,
    Traitor,
    Queen,
    Servant,
    Madmate,
    Umbrer,
    Doll,
	Hatter,
	Artist,
	Lawbreaker,
	Tucker,
	Chimera,
	IronMate,
	Monika,
	Heretic,

	Xion,
}

public enum CombinationRoleType : byte
{
    Avalon,
    HeroAca,
    DetectiveOffice,
    Kids,

    Lover,
    Buddy,

    Sharer,

    Supporter,
    Guesser,
    Mover,
	Accelerator,
	Skater,

	Traitor,
}

public enum RoleGameOverReason
{
    AssassinationMarin = 20,
	TeroristoTeroWithShip,

    AliceKilledByImposter,
    AliceKillAllOther,

    JackalKillAllOther,

    LoverKillAllOther,
    ShipFallInLove,

    TaskMasterGoHome,

    MissionaryAllAgainstGod,

    JesterMeetingFavorite,

    YandereKillAllOther,
    YandereShipJustForTwo,

    VigilanteKillAllOther,
    VigilanteNewIdealWorld,

    YokoAllDeceive,

    MinerExplodeEverything,

    EaterAllEatInTheShip,
    EaterAliveAlone,

    TraitorKillAllOther,

    QueenKillAllOther,

    UmbrerBiohazard,

    KidsTooBigHomeAlone,
    KidsAliveAlone,

	HatterEndlessTeaTime,
	HatterTeaPartyTime,

	ArtistShipToArt,

	TuckerShipIsExperimentStation,
	MonikaThisGameIsMine,
	MonikaIamTheOnlyOne,

	UnKnown = 100,
}

public enum NeutralSeparateTeam
{
    Jackal,
    Alice,
    Lover,
    Missionary,
    Yandere,
    Vigilante,
    Miner,
    Eater,
    Traitor,
    Queen,
    Kids,
	Tucker,
	Monika
}

public static class ExtremeRoleManager
{
    public const int OptionOffsetPerRole = 200;

	public const int RoleCategoryIdOffset = 200;
	private const int conbRoleIdOffset = 1000;

	public static readonly IReadOnlySet<ExtremeRoleId> SpecialWinCheckRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Lover,
        ExtremeRoleId.Vigilante,
        ExtremeRoleId.Delinquent,

		ExtremeRoleId.Yandere,
		ExtremeRoleId.Hatter,
		ExtremeRoleId.Monika,
	};

    public static readonly ImmutableDictionary<int, SingleRoleBase> NormalRole =
		new Dictionary<int, SingleRoleBase>()
        {
            {(int)ExtremeRoleId.SpecialCrew , new SpecialCrew()},
            {(int)ExtremeRoleId.Sheriff     , new Sheriff()},
            {(int)ExtremeRoleId.Maintainer  , new Maintainer()},
            {(int)ExtremeRoleId.Neet        , new Neet()},
            {(int)ExtremeRoleId.Watchdog    , new Watchdog()},
            {(int)ExtremeRoleId.Supervisor  , new Supervisor()},
            {(int)ExtremeRoleId.BodyGuard   , new BodyGuard()},
            {(int)ExtremeRoleId.Whisper     , new Whisper()},
            {(int)ExtremeRoleId.TimeMaster  , new TimeMaster()},
            {(int)ExtremeRoleId.Agency      , new Agency()},
            {(int)ExtremeRoleId.Bakary      , new Bakary()},
            {(int)ExtremeRoleId.CurseMaker  , new CurseMaker()},
            {(int)ExtremeRoleId.Fencer      , new Fencer()},
            {(int)ExtremeRoleId.Opener      , new Opener()},
            {(int)ExtremeRoleId.Carpenter   , new Carpenter()},
            {(int)ExtremeRoleId.Survivor    , new Survivor()},
            {(int)ExtremeRoleId.Captain     , new Captain()},
            {(int)ExtremeRoleId.Photographer, new Photographer()},
            {(int)ExtremeRoleId.Delusioner  , new Delusioner()},
            {(int)ExtremeRoleId.Resurrecter , new Resurrecter()},
            {(int)ExtremeRoleId.Gambler     , new Gambler()},
            {(int)ExtremeRoleId.Teleporter  , new Teleporter()},
			{(int)ExtremeRoleId.Moderator   , new Moderator()},
			{(int)ExtremeRoleId.Psychic     , new Psychic()},
			{(int)ExtremeRoleId.Bait        , new Bait()},
			{(int)ExtremeRoleId.Jailer      , new Jailer()},
			{(int)ExtremeRoleId.Summoner    , new Summoner()},

			{(int)ExtremeRoleId.SpecialImpostor, new SpecialImpostor()},
            {(int)ExtremeRoleId.Evolver        , new Evolver()},
            {(int)ExtremeRoleId.Carrier        , new Carrier()},
            {(int)ExtremeRoleId.PsychoKiller   , new PsychoKiller()},
            {(int)ExtremeRoleId.BountyHunter   , new BountyHunter()},
            {(int)ExtremeRoleId.Painter        , new Painter()},
            {(int)ExtremeRoleId.Faker          , new Faker()},
            {(int)ExtremeRoleId.OverLoader     , new OverLoader()},
            {(int)ExtremeRoleId.Cracker        , new Cracker()},
            {(int)ExtremeRoleId.Bomber         , new Bomber()},
            {(int)ExtremeRoleId.Mery           , new Mery()},
            {(int)ExtremeRoleId.SlaveDriver    , new SlaveDriver()},
            {(int)ExtremeRoleId.SandWorm       , new SandWorm()},
            {(int)ExtremeRoleId.Smasher        , new Smasher()},
            {(int)ExtremeRoleId.AssaultMaster  , new AssaultMaster()},
            {(int)ExtremeRoleId.Shooter        , new Shooter()},
            {(int)ExtremeRoleId.LastWolf       , new LastWolf()},
            {(int)ExtremeRoleId.Commander      , new Commander()},
            {(int)ExtremeRoleId.Hypnotist      , new Hypnotist()},
            {(int)ExtremeRoleId.UnderWarper    , new UnderWarper()},
            {(int)ExtremeRoleId.Magician       , new Magician()},
            {(int)ExtremeRoleId.Slime          , new Slime()},
            {(int)ExtremeRoleId.Zombie         , new Zombie()},
			{(int)ExtremeRoleId.Thief          , new Thief()},
			{(int)ExtremeRoleId.Crewshroom     , new Crewshroom()},
			{(int)ExtremeRoleId.Terorist       , new Terorist()},
			{(int)ExtremeRoleId.Raider         , new Raider()},
			{(int)ExtremeRoleId.Glitch         , new Glitch()},
			{(int)ExtremeRoleId.Hijacker       , new Hijacker()},
			{(int)ExtremeRoleId.TimeBreaker    , new TimeBreaker()},

			{(int)ExtremeRoleId.Alice     , new Alice()},
            {(int)ExtremeRoleId.Jackal    , new Jackal()},
            {(int)ExtremeRoleId.TaskMaster, new TaskMaster()},
            {(int)ExtremeRoleId.Missionary, new Missionary()},
            {(int)ExtremeRoleId.Jester    , new Jester()},
            {(int)ExtremeRoleId.Yandere   , new Yandere()},
            {(int)ExtremeRoleId.Yoko      , new Yoko()},
            {(int)ExtremeRoleId.Totocalcio, new Totocalcio()},
            {(int)ExtremeRoleId.Miner     , new Miner()},
            {(int)ExtremeRoleId.Eater     , new Eater()},
            {(int)ExtremeRoleId.Queen     , new Queen()},
            {(int)ExtremeRoleId.Madmate   , new Madmate()},
            {(int)ExtremeRoleId.Umbrer    , new Umbrer()},
			{(int)ExtremeRoleId.Hatter    , new Hatter()},
			{(int)ExtremeRoleId.Artist    , new Artist()},
			{(int)ExtremeRoleId.Tucker    , new Tucker()},
			{(int)ExtremeRoleId.IronMate  , new IronMate()},
			{(int)ExtremeRoleId.Monika    , new Monika()},
		}.ToImmutableDictionary();

    public static readonly ImmutableDictionary<byte, CombinationRoleManagerBase> CombRole =
		new Dictionary<byte, CombinationRoleManagerBase>()
        {
            {(byte)CombinationRoleType.Avalon         , new Avalon()},
            {(byte)CombinationRoleType.HeroAca        , new HeroAcademia()},
            {(byte)CombinationRoleType.DetectiveOffice, new DetectiveOffice()},
            {(byte)CombinationRoleType.Kids           , new Kids()},
            {(byte)CombinationRoleType.Buddy          , new BuddyManager()},
            {(byte)CombinationRoleType.Lover          , new LoverManager()},
            {(byte)CombinationRoleType.Sharer         , new SharerManager()},
            {(byte)CombinationRoleType.Supporter      , new SupporterManager()},
            {(byte)CombinationRoleType.Guesser        , new GuesserManager()},
            {(byte)CombinationRoleType.Mover          , new MoverManager()},
			{(byte)CombinationRoleType.Accelerator    , new AcceleratorManager()},
			{(byte)CombinationRoleType.Skater         , new SkaterManager()},
			{(byte)CombinationRoleType.Traitor        , new TraitorManager()},
        }.ToImmutableDictionary();

    public static readonly Dictionary<byte, SingleRoleBase> GameRole = new();

    public static readonly IReadOnlySet<ExtremeRoleId> WinCheckDisableRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Jackal,
        ExtremeRoleId.Assassin,
        ExtremeRoleId.Hero,
        ExtremeRoleId.Villain,
		ExtremeRoleId.Yoko,
		ExtremeRoleId.Chimera,
    };

    public enum ReplaceOperation : byte
    {
        ResetVanillaRole = 0,
        ForceReplaceToSidekick,
        SidekickToJackal,
        CreateServant,
		ForceReplaceToYardbird,
		BecomeLawbreaker,
		ForceRelaceToChimera,
		RemoveChimera,
	}

	public static int GetRoleGroupId(ExtremeRoleId roleId)
		=> RoleCategoryIdOffset + (int)roleId;

	public static int GetCombRoleGroupId(CombinationRoleType roleId)
		=> conbRoleIdOffset + (int)roleId;

	public static void CreateCombinationRoleOptions()
    {
        foreach (var role in CombRole.Values)
        {
			role.CreateRoleAllOption();
        }
    }

    public static void CreateNormalRoleOptions()
    {
        foreach (var role in NormalRole.Values)
        {
			role.CreateRoleAllOption();
        }
    }

    public static void Initialize()
    {
        GameRole.Clear();
        foreach (var role in CombRole.Values)
        {
            role.Initialize();
        }

        // 各種役職のリセット
        // シオンのリセット
        Xion.Purge();
        // ボディーガードのリセット
        BodyGuard.ResetAllShild();
        // タイムマスターのリセット
        TimeMaster.ResetHistory();

        // APIのステータスのリセット
        API.Extension.State.RoleState.Reset();

    }

    public static bool IsDisableWinCheckRole(SingleRoleBase role)
    {
        bool mainRoleCheckResult = WinCheckDisableRole.Contains(role.Id);
        if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is not null)
        {
			return
				mainRoleCheckResult ||
				WinCheckDisableRole.Contains(multiAssignRole.AnotherRole.Id);
		}
        else
        {
			return mainRoleCheckResult;
        }
    }
	public static bool IsAliveWinNeutral(
		SingleRoleBase role, NetworkedPlayerInfo playerInfo)
		=> role.Id switch
		{
			ExtremeRoleId.Neet => !(playerInfo.IsDead || playerInfo.Disconnected),
			_ => false
		};

    public static SingleRoleBase GetLocalPlayerRole()
    {
		if (!TryGetRole(PlayerControl.LocalPlayer.PlayerId, out var role))
		{
			throw new ArgumentNullException("Local Role is Null!!!!!!!!!!");
		}
		return role;
    }

    public static void SetPlayerIdToMultiRoleId(
        byte combType, int roleId, byte playerId, int id, byte bytedRoleType)
    {
        RoleTypes roleType = (RoleTypes)bytedRoleType;

		bool hasVanilaRole = roleType is
			RoleTypes.Engineer or
			RoleTypes.Scientist or
			RoleTypes.Shapeshifter or
			RoleTypes.Noisemaker or
			RoleTypes.Phantom or
			RoleTypes.Tracker;

		var role = CombRole[combType].GetRole(roleId, roleType);

        if (role is null)
		{
			return;
		}

		SingleRoleBase addRole = role.Clone();

		if (addRole is IRoleAbility abilityRole &&
			PlayerControl.LocalPlayer.PlayerId == playerId)
		{
			Helper.Logging.Debug("Try Create Ability NOW!!!");
			abilityRole.CreateAbility();
		}

		addRole.Initialize();
		addRole.SetControlId(id);

		SetNewRole(playerId, addRole);

		if (hasVanilaRole)
		{
			SetNewAnothorRole(playerId, new Solo.VanillaRoleWrapper(roleType));
		}
		Helper.Logging.Debug($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
	}
    public static void SetPlyerIdToSingleRoleId(
        int roleId, byte playerId, int controlId)
    {

        if (!Enum.IsDefined(typeof(RoleTypes), Convert.ToUInt16(roleId)))
        {
            SingleRoleBase role;
            if (roleId != (int)ExtremeRoleId.Xion)
            {
                role = NormalRole[roleId];
            }
            else
            {
                role = new Xion(playerId);
            }

            setPlyerIdToSingleRole(playerId, role, controlId);
        }
        else
        {
            setPlyerIdToSingleRole(
                playerId,
                new Solo.VanillaRoleWrapper((RoleTypes)roleId),
                controlId);
        }
    }

    public static void RoleReplace(
        byte caller, byte targetId, ReplaceOperation ops)
    {
        switch(ops)
        {
            case ReplaceOperation.ResetVanillaRole:
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(
                    Helper.Player.GetPlayerControlById(targetId),
                    RoleTypes.Crewmate);
                break;
            case ReplaceOperation.ForceReplaceToSidekick:
                Jackal.TargetToSideKick(caller, targetId);
                break;
            case ReplaceOperation.SidekickToJackal:
                Sidekick.BecomeToJackal(caller, targetId);
                break;
            case ReplaceOperation.CreateServant:
                Queen.TargetToServant(caller, targetId);
                break;
			case ReplaceOperation.ForceReplaceToYardbird:
				Jailer.NotCrewmateToYardbird(caller, targetId);
				break;
			case ReplaceOperation.BecomeLawbreaker:
				if (caller != targetId)
				{
					return;
				}
				Jailer.ToLawbreaker(caller);
				break;
			case ReplaceOperation.ForceRelaceToChimera:
				Tucker.TargetToChimera(caller, targetId);
				break;
			case ReplaceOperation.RemoveChimera:
				Tucker.RemoveChimera(caller, targetId);
				break;
			default:
                break;
        }
    }

    public static void SetNewRole(byte playerId, SingleRoleBase newRole)
    {
        lock (GameRole)
        {
            GameRole[playerId] =  newRole;
            ExtremeRolesPlugin.ShipState.AddGlobalActionRole(newRole);
        }
    }

    public static void SetNewAnothorRole(byte playerId, SingleRoleBase newRole)
    {
        ((MultiAssignRoleBase)GameRole[playerId]).SetAnotherRole(newRole);
        ExtremeRolesPlugin.ShipState.AddGlobalActionRole(newRole);
    }

    private static void setPlyerIdToSingleRole(
        byte playerId, SingleRoleBase role, int controlId)
    {

        SingleRoleBase addRole = role.Clone();

        if (addRole is IRoleAbility abilityRole &&
            PlayerControl.LocalPlayer.PlayerId == playerId)
        {
            Helper.Logging.Debug("Try Create Ability NOW!!!");
            abilityRole.CreateAbility();
        }

        addRole.Initialize();
        addRole.SetControlId(controlId);

        if (!GameRole.ContainsKey(playerId))
        {
            SetNewRole(playerId, addRole);
        }
        else
        {
            SetNewAnothorRole(playerId, addRole);

            if (TryGetRole(playerId, out var existRole) &&
				existRole is IRoleAbility multiAssignAbilityRole &&
                PlayerControl.LocalPlayer.PlayerId == playerId &&
				multiAssignAbilityRole.Button != null)
            {
				multiAssignAbilityRole.Button.HotKey = UnityEngine.KeyCode.C;
			}
        }
        Helper.Logging.Debug($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
    }

// TryGet系列：ここでTrueの場合、取得した役職はNullではない！！！！
	public static bool TryGetRole(byte playerId, [NotNullWhen(true)] out SingleRoleBase? role )
		=> GameRole.TryGetValue(playerId, out role) && role is not null;

	public static bool TryGetSafeCastedRole<T>(byte playerId, [NotNullWhen(true)] out T? role) where T : SingleRoleBase
	{
		role = null;
		if (!TryGetRole(playerId, out var checkRole))
		{
			return false;
		}
		role = safeCast<T>(checkRole);
		return role is not null;
	}

	public static bool TryGetSafeCastedLocalRole<T>([NotNullWhen(true)] out T? role) where T : SingleRoleBase
	{
		var rowRole = GetLocalPlayerRole();
		role = safeCast<T>(rowRole);
		return role is not null;
	}

	public static T? GetSafeCastedRole<T>(byte playerId) where T : SingleRoleBase
    {
        TryGetRole(playerId, out var checkRole);
		return safeCast<T>(checkRole);
	}

    public static T? GetSafeCastedLocalPlayerRole<T>() where T : SingleRoleBase
    {
        var checkRole = GetLocalPlayerRole();
        return safeCast<T>(checkRole);
    }

    public static (T?, T?) GetInterfaceCastedLocalRole<T>() where T : class
    {
		var checkRole = GetLocalPlayerRole();
		return dualSafeCast<T>(checkRole);
	}

    public static (T?, T?) GetInterfaceCastedRole<T>(byte playerId) where T : class
    {
		TryGetRole(playerId, out var checkRole);
		return dualSafeCast<T>(checkRole);
	}


	private static T? safeCast<T>(in SingleRoleBase? checkRole) where T : SingleRoleBase
	{
		if (checkRole is T role)
		{
			return role;
		}

		if (checkRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is T anotherRole)
		{
			return anotherRole;
		}

		return null;
	}
	private static (T?, T?) dualSafeCast<T>(in SingleRoleBase? checkRole) where T : class
	{
		T? interfacedSingleRole = checkRole as T;
		T? interfacedMultiRole = null;

		if (checkRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is T anotherRole)
		{
			interfacedMultiRole = anotherRole;
		}

		return (interfacedSingleRole, interfacedMultiRole);
	}
}
