using System;
using System.Collections.Generic;
using System.Linq;

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

    Traitor,
}

public enum RoleGameOverReason
{
    AssassinationMarin = 20,
    
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
    Kids
}

public static class ExtremeRoleManager
{
    public const int OptionOffsetPerRole = 50;

    public static readonly HashSet<ExtremeRoleId> SpecialWinCheckRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Lover,
        ExtremeRoleId.Yandere,
        ExtremeRoleId.Vigilante,
        ExtremeRoleId.Delinquent,
    };

    public static readonly Dictionary<
        int, SingleRoleBase> NormalRole = new Dictionary<int, SingleRoleBase>()
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
        };

    public static readonly Dictionary<
        byte, CombinationRoleManagerBase> CombRole = new Dictionary<byte, CombinationRoleManagerBase>()
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
            {(byte)CombinationRoleType.Traitor        , new TraitorManager()},
        };

    public static Dictionary<
        byte, SingleRoleBase> GameRole = new Dictionary<byte, SingleRoleBase> ();

    public static readonly HashSet<ExtremeRoleId> WinCheckDisableRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Jackal,
        ExtremeRoleId.Assassin,
        ExtremeRoleId.Hero,
        ExtremeRoleId.Villain
    };

    public enum ReplaceOperation : byte
    {
        ResetVanillaRole = 0,
        ForceReplaceToSidekick,
        SidekickToJackal,
        CreateServant,
    }

    public static void CreateCombinationRoleOptions(
        int optionIdOffsetChord)
    {

        if (CombRole.Count == 0) { return; };

        IEnumerable<CombinationRoleManagerBase> roles = CombRole.Values;

        int roleOptionOffset = optionIdOffsetChord;

        foreach (var item
         in roles.Select((Value, Index) => new { Value, Index }))
        {
            roleOptionOffset = roleOptionOffset + (
                OptionOffsetPerRole * (item.Index + item.Value.Roles.Count + 1));
            item.Value.CreateRoleAllOption(roleOptionOffset);
        }
    }

    public static void CreateNormalRoleOptions(
        int optionIdOffsetChord)
    {

        if (NormalRole.Count == 0) { return; };

        IEnumerable<SingleRoleBase> roles = NormalRole.Values;

        int roleOptionOffset = 0;

        foreach (var item in roles.Select(
            (Value, Index) => new { Value, Index }))
        {
            roleOptionOffset = optionIdOffsetChord + (OptionOffsetPerRole * item.Index);
            item.Value.CreateRoleAllOption(roleOptionOffset);
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
        var mainRoleCheckResult = WinCheckDisableRole.Contains(role.Id);
        var multiAssignRole = role as MultiAssignRoleBase;
        if (multiAssignRole == null)
        {
            return mainRoleCheckResult;
        }
        else
        {
            if (multiAssignRole.AnotherRole != null)
            {
                return WinCheckDisableRole.Contains(multiAssignRole.AnotherRole.Id);
            }
            else
            {
                return mainRoleCheckResult;
            }
        }
    }
    public static bool IsAliveWinNeutral(
        SingleRoleBase role, GameData.PlayerInfo playerInfo)
    {
        bool isAlive = (!playerInfo.IsDead && !playerInfo.Disconnected);

        if (role.Id == ExtremeRoleId.Neet && isAlive) { return true; }

        return false;
    }

    public static SingleRoleBase GetLocalPlayerRole()
    {
        return GameRole[CachedPlayerControl.LocalPlayer.PlayerId];
    }

    public static void SetPlayerIdToMultiRoleId(
        byte combType, int roleId, byte playerId, int id, byte bytedRoleType)
    {
        RoleTypes roleType = (RoleTypes)bytedRoleType;

        bool hasVanilaRole = false;

        switch (roleType)
        {
            case RoleTypes.Scientist:
            case RoleTypes.Engineer:
            case RoleTypes.Shapeshifter:
                hasVanilaRole = true;
                break;
            default:
                break;
        }

        var role = CombRole[combType].GetRole(
                roleId, roleType);

        if (role != null)
        {

            SingleRoleBase addRole = role.Clone();

            if (addRole is IRoleAbility abilityRole && 
                CachedPlayerControl.LocalPlayer.PlayerId == playerId)
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
            CachedPlayerControl.LocalPlayer.PlayerId == playerId)
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

            if (GameRole[playerId] is IRoleAbility multiAssignAbilityRole &&
                CachedPlayerControl.LocalPlayer.PlayerId == playerId)
            {
                if (multiAssignAbilityRole.Button != null)
                {
                    multiAssignAbilityRole.Button.SetHotKey(UnityEngine.KeyCode.C);
                }
            }
        }
        Helper.Logging.Debug($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
    }

    public static T GetSafeCastedRole<T>(byte playerId) where T : SingleRoleBase
    {
        var checRole = GameRole[playerId];

        var role = checRole as T;
        
        if (role != null)
        {
            return role;
        }

        var multiAssignRole = checRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            role = multiAssignRole.AnotherRole as T;

            if (role != null)
            {
                return role;
            }
        }

        return null;

    }

    public static T GetSafeCastedLocalPlayerRole<T>() where T : SingleRoleBase
    {
        var checkRole = GetLocalPlayerRole();

        var role = checkRole as T;

        if (role != null)
        {
            return role;
        }

        var multiAssignRole = checkRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            role = multiAssignRole.AnotherRole as T;

            if (role != null)
            {
                return role;
            }
        }

        return null;

    }

    public static (T, T) GetInterfaceCastedLocalRole<T>() where T : class
    {
        SingleRoleBase checkRole = GameRole[CachedPlayerControl.LocalPlayer.PlayerId];

        T interfacedSingleRole = checkRole as T;
        T interfacedMultiRole = null;

        MultiAssignRoleBase multiAssignRole = checkRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            interfacedMultiRole = multiAssignRole.AnotherRole as T;
        }

        return (interfacedSingleRole, interfacedMultiRole);
    }

    public static (T, T) GetInterfaceCastedRole<T>(byte playerId) where T : class
    {
        SingleRoleBase checkRole = GameRole[playerId];

        T interfacedSingleRole = checkRole as T;
        T interfacedMultiRole = null;

        MultiAssignRoleBase multiAssignRole = checkRole as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            interfacedMultiRole = multiAssignRole.AnotherRole as T;
        }

        return (interfacedSingleRole, interfacedMultiRole);
    }

}
