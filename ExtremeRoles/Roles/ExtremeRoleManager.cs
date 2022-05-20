using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral;
using ExtremeRoles.Roles.Solo.Impostor;


namespace ExtremeRoles.Roles
{
    public enum ExtremeRoleId
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
            byte, SingleRoleBase> NormalRole = new Dictionary<byte, SingleRoleBase>()
            {
                {(byte)ExtremeRoleId.SpecialCrew, new SpecialCrew()},
                {(byte)ExtremeRoleId.Sheriff    , new Sheriff()},
                {(byte)ExtremeRoleId.Maintainer , new Maintainer()},
                {(byte)ExtremeRoleId.Neet       , new Neet()},
                {(byte)ExtremeRoleId.Watchdog   , new Watchdog()},
                {(byte)ExtremeRoleId.Supervisor , new Supervisor()},
                {(byte)ExtremeRoleId.BodyGuard  , new BodyGuard()},
                {(byte)ExtremeRoleId.Whisper    , new Whisper()},
                {(byte)ExtremeRoleId.TimeMaster , new TimeMaster()},
                {(byte)ExtremeRoleId.Agency     , new Agency()},
                {(byte)ExtremeRoleId.Bakary     , new Bakary()},
                {(byte)ExtremeRoleId.CurseMaker , new CurseMaker()},
                {(byte)ExtremeRoleId.Fencer     , new Fencer()},
                {(byte)ExtremeRoleId.Opener     , new Opener()},
                {(byte)ExtremeRoleId.Carpenter  , new Carpenter()},

                {(byte)ExtremeRoleId.SpecialImpostor, new SpecialImpostor()},
                {(byte)ExtremeRoleId.Evolver        , new Evolver()},
                {(byte)ExtremeRoleId.Carrier        , new Carrier()},
                {(byte)ExtremeRoleId.PsychoKiller   , new PsychoKiller()},
                {(byte)ExtremeRoleId.BountyHunter   , new BountyHunter()},
                {(byte)ExtremeRoleId.Painter        , new Painter()},
                {(byte)ExtremeRoleId.Faker          , new Faker()},
                {(byte)ExtremeRoleId.OverLoader     , new OverLoader()},
                {(byte)ExtremeRoleId.Cracker        , new Cracker()},
                {(byte)ExtremeRoleId.Bomber         , new Bomber()},
                {(byte)ExtremeRoleId.Mery           , new Mery()},
                {(byte)ExtremeRoleId.SlaveDriver    , new SlaveDriver()},
                {(byte)ExtremeRoleId.SandWorm       , new SandWorm()},
                {(byte)ExtremeRoleId.Smasher        , new Smasher()},
                {(byte)ExtremeRoleId.AssaultMaster  , new AssaultMaster()},

                {(byte)ExtremeRoleId.Alice     , new Alice()},
                {(byte)ExtremeRoleId.Jackal    , new Jackal()},
                {(byte)ExtremeRoleId.TaskMaster, new TaskMaster()},
                {(byte)ExtremeRoleId.Missionary, new Missionary()},
                {(byte)ExtremeRoleId.Jester    , new Jester()},
                {(byte)ExtremeRoleId.Yandere   , new Yandere()},
                {(byte)ExtremeRoleId.Yoko      , new Yoko()},
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
            IEnumerable<CombinationRoleManagerBase> roles = CombRole.Values;

            if (roles.Count() == 0) { return; };

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

            IEnumerable<SingleRoleBase> roles = NormalRole.Values;

            if (roles.Count() == 0) { return; };

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
            return GameRole[PlayerControl.LocalPlayer.PlayerId];
        }

        public static void SetPlayerIdToMultiRoleId(
            byte combType, byte roleId, byte playerId, byte id, byte bytedRoleType)
        {
            RoleTypes roleType = (RoleTypes)bytedRoleType;
            bool hasVanilaRole = roleType != RoleTypes.Crewmate || roleType != RoleTypes.Impostor;

            var role = CombRole[combType].GetRole(
                    roleId, roleType);

            if (role != null)
            {

                SingleRoleBase addRole = role.Clone();

                IRoleAbility abilityRole = addRole as IRoleAbility;

                if (abilityRole != null && PlayerControl.LocalPlayer.PlayerId == playerId)
                {
                    Helper.Logging.Debug("Try Create Ability NOW!!!");
                    abilityRole.CreateAbility();
                }

                addRole.Initialize();
                addRole.GameControlId = id;
                roleControlId = id + 1;

                GameRole.Add(
                    playerId, addRole);

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
            byte roleId, byte playerId)
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

            if (abilityRole != null && PlayerControl.LocalPlayer.PlayerId == playerId)
            {
                Helper.Logging.Debug("Try Create Ability NOW!!!");
                abilityRole.CreateAbility();
            }

            addRole.Initialize();
            addRole.GameControlId = roleControlId;
            roleControlId = roleControlId + 1;

            if (!GameRole.ContainsKey(playerId))
            {
                GameRole.Add(
                    playerId, addRole);

            }
            else
            {
                ((MultiAssignRoleBase)GameRole[
                    playerId]).SetAnotherRole(addRole);
                
                IRoleAbility multiAssignAbilityRole = ((MultiAssignRoleBase)GameRole[
                    playerId]) as IRoleAbility;

                if (multiAssignAbilityRole != null && PlayerControl.LocalPlayer.PlayerId == playerId)
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
            var role = GameRole[playerId] as T;
            
            if (role != null)
            {
                return role;
            }

            var multiAssignRole = GameRole[playerId] as MultiAssignRoleBase;
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
            var role = GameRole[PlayerControl.LocalPlayer.PlayerId] as T;

            if (role != null)
            {
                return role;
            }

            var multiAssignRole = GameRole[PlayerControl.LocalPlayer.PlayerId] as MultiAssignRoleBase;
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
