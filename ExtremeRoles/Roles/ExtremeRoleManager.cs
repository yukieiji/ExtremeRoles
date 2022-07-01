using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral;
using ExtremeRoles.Roles.Solo.Impostor;

using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles
{
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

        Alice,
        Jackal,
        Sidekick,
        TaskMaster,
        Missionary,
        Jester,
        Yandere,
        Yoko
    }
    public enum CombinationRoleType
    {
        Avalon,
        HeroAca,
        DetectiveOffice,
        Lover,
        Supporter,
        Sharer,
    }

    public enum RoleGameOverReason
    {
        AssassinationMarin = 10,
        
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
    }

    public static class ExtremeRoleManager
    {
        public const int OptionOffsetPerRole = 50;

        public static readonly HashSet<ExtremeRoleId> SpecialWinCheckRole = new HashSet<ExtremeRoleId>()
        {
            ExtremeRoleId.Lover,
            ExtremeRoleId.Yandere,
            ExtremeRoleId.Vigilante,
        };

        public static readonly Dictionary<
            int, SingleRoleBase> NormalRole = new Dictionary<int, SingleRoleBase>()
            {
                {(int)ExtremeRoleId.SpecialCrew, new SpecialCrew()},
                {(int)ExtremeRoleId.Sheriff    , new Sheriff()},
                {(int)ExtremeRoleId.Maintainer , new Maintainer()},
                {(int)ExtremeRoleId.Neet       , new Neet()},
                {(int)ExtremeRoleId.Watchdog   , new Watchdog()},
                {(int)ExtremeRoleId.Supervisor , new Supervisor()},
                {(int)ExtremeRoleId.BodyGuard  , new BodyGuard()},
                {(int)ExtremeRoleId.Whisper    , new Whisper()},
                {(int)ExtremeRoleId.TimeMaster , new TimeMaster()},
                {(int)ExtremeRoleId.Agency     , new Agency()},
                {(int)ExtremeRoleId.Bakary     , new Bakary()},
                {(int)ExtremeRoleId.CurseMaker , new CurseMaker()},
                {(int)ExtremeRoleId.Fencer     , new Fencer()},
                {(int)ExtremeRoleId.Opener     , new Opener()},
                {(int)ExtremeRoleId.Carpenter  , new Carpenter()},
                {(int)ExtremeRoleId.Survivor   , new Survivor()},

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

                {(int)ExtremeRoleId.Alice     , new Alice()},
                {(int)ExtremeRoleId.Jackal    , new Jackal()},
                {(int)ExtremeRoleId.TaskMaster, new TaskMaster()},
                {(int)ExtremeRoleId.Missionary, new Missionary()},
                {(int)ExtremeRoleId.Jester    , new Jester()},
                {(int)ExtremeRoleId.Yandere   , new Yandere()},
                {(int)ExtremeRoleId.Yoko      , new Yoko()},
            };

        public static readonly Dictionary<
            byte, CombinationRoleManagerBase> CombRole = new Dictionary<byte, CombinationRoleManagerBase>()
            {
                {(byte)CombinationRoleType.Avalon         , new Avalon()},
                {(byte)CombinationRoleType.HeroAca        , new HeroAcademia()},
                {(byte)CombinationRoleType.DetectiveOffice, new DetectiveOffice()},
                {(byte)CombinationRoleType.Lover          , new LoverManager()},
                {(byte)CombinationRoleType.Supporter      , new SupporterManager()},
                {(byte)CombinationRoleType.Sharer         , new SharerManager()   },
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

        private static int roleControlId = 0;

        public enum ReplaceOperation
        {
            ForceReplaceToSidekick = 0,
            SidekickToJackal,
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
            roleControlId = 0;
            GameRole.Clear();
            foreach (var role in CombRole.Values)
            {
                role.Initialize();
            }
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
            byte combType, int roleId, byte playerId, byte id, byte bytedRoleType)
        {
            RoleTypes roleType = (RoleTypes)bytedRoleType;
            bool hasVanilaRole = roleType != RoleTypes.Crewmate || roleType != RoleTypes.Impostor;

            var role = CombRole[combType].GetRole(
                    roleId, roleType);

            if (role != null)
            {

                SingleRoleBase addRole = role.Clone();

                IRoleAbility abilityRole = addRole as IRoleAbility;

                if (abilityRole != null && CachedPlayerControl.LocalPlayer.PlayerId == playerId)
                {
                    Helper.Logging.Debug("Try Create Ability NOW!!!");
                    abilityRole.CreateAbility();
                }

                addRole.Initialize();
                addRole.GameControlId = id;
                roleControlId = id + 1;
                lock (GameRole)
                {
                    GameRole.Add(playerId, addRole);
                }

                if (hasVanilaRole)
                {
                    ((MultiAssignRoleBase)GameRole[
                        playerId]).SetAnotherRole(
                            new Solo.VanillaRoleWrapper(roleType));
                }
                Helper.Logging.Debug($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
            }
        }
        public static void SetPlyerIdToSingleRoleId(
            int roleId, byte playerId)
        {
            if (!Enum.IsDefined(typeof(RoleTypes), Convert.ToUInt16(roleId)))
            {
                setPlyerIdToSingleRole(playerId, NormalRole[roleId]);
            }
            else
            {
                setPlyerIdToSingleRole(
                    playerId,
                    new Solo.VanillaRoleWrapper((RoleTypes)roleId));
            }
        }

        public static void RoleReplace(
            byte caller, byte targetId, ReplaceOperation ops)
        {
            switch(ops)
            {
                case ReplaceOperation.ForceReplaceToSidekick:
                    Jackal.TargetToSideKick(caller, targetId);
                    break;
                case ReplaceOperation.SidekickToJackal:
                    Sidekick.BecomeToJackal(caller, targetId);
                    break;
                default:
                    break;
            }
        }

        private static void setPlyerIdToSingleRole(
            byte playerId, SingleRoleBase role)
        {

            SingleRoleBase addRole = role.Clone();

            IRoleAbility abilityRole = addRole as IRoleAbility;

            if (abilityRole != null && CachedPlayerControl.LocalPlayer.PlayerId == playerId)
            {
                Helper.Logging.Debug("Try Create Ability NOW!!!");
                abilityRole.CreateAbility();
            }

            addRole.Initialize();
            addRole.GameControlId = roleControlId;
            roleControlId = roleControlId + 1;

            if (!GameRole.ContainsKey(playerId))
            {
                lock (GameRole)
                {
                    GameRole.Add(playerId, addRole);
                }
            }
            else
            {
                ((MultiAssignRoleBase)GameRole[
                    playerId]).SetAnotherRole(addRole);
                
                IRoleAbility multiAssignAbilityRole = ((MultiAssignRoleBase)GameRole[
                    playerId]) as IRoleAbility;

                if (multiAssignAbilityRole != null && CachedPlayerControl.LocalPlayer.PlayerId == playerId)
                {
                    if (multiAssignAbilityRole.Button != null)
                    {
                        multiAssignAbilityRole.Button.PositionOffset = new UnityEngine.Vector3(0, 2.6f, 0);
                        multiAssignAbilityRole.Button.ReplaceHotKey(UnityEngine.KeyCode.G);
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
                if (multiAssignRole.AnotherRole != null)
                {
                    role = multiAssignRole.AnotherRole as T;

                    if (role != null)
                    {
                        return role;
                    }
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
                if (multiAssignRole.AnotherRole != null)
                {
                    role = multiAssignRole.AnotherRole as T;

                    if (role != null)
                    {
                        return role;
                    }
                }
            }

            return null;

        }

    }
}
