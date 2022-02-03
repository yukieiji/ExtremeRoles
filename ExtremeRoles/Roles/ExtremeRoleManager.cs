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
        
        SpecialCrew,
        Sheriff,
        Maintainer,
        Neet,
        Watchdog,
        Supervisor,
        BodyGuard,

        SpecialImpostor,
        Evolver,
        Carrier,
        PsychoKiller,
        BountyHunter,
        Painter,
        OverLoader,

        Alice,
        Jackal,
        Sidekick,
        TaskMaster,
        Missionary,
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

        UnKnown = 100,
    }

    public enum NeutralSeparateTeam
    {
        Jackal,
        Alice,
        Lover,
        Missionary
    }

    public static class ExtremeRoleManager
    {
        public const int OptionOffsetPerRole = 50;

        public static readonly List<ExtremeRoleId> SpecialWinCheckRole = new List<ExtremeRoleId>()
        {
            ExtremeRoleId.Lover,
        };

        public static readonly List<
            SingleRoleBase> NormalRole = new List<SingleRoleBase>()
            {
                new SpecialCrew(),
                new Sheriff(),
                new Maintainer(),
                new Neet(),
                new Watchdog(),
                new Supervisor(),
                new BodyGuard(),

                new SpecialImpostor(),
                new Evolver(),
                new Carrier(),
                new PsychoKiller(),
                new BountyHunter(),
                new Painter(),
                new OverLoader(),

                new Alice(),
                new Jackal(),
                new TaskMaster(),
                new Missionary(),
            };

        public static readonly List<
            CombinationRoleManagerBase> CombRole = new List<CombinationRoleManagerBase>()
            {
                new Avalon(),
                new LoverManager(),
                new SupporterManager(),
            };

        public static Dictionary<
            byte, SingleRoleBase> GameRole = new Dictionary<byte, SingleRoleBase> ();

        private static int roleControlId = 0;

        public enum ReplaceOperation
        {
            ForceReplaceToSidekick = 0,
            SidekickToJackal,
        }

        public static void CreateCombinationRoleOptions(
            int optionIdOffsetChord)
        {
            IEnumerable<CombinationRoleManagerBase> roles = CombRole;

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

            IEnumerable<SingleRoleBase> roles = NormalRole;

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
            foreach (var role in CombRole)
            {
                role.Initialize();
            }
        }

        public static bool IsDisableWinCheckRole(SingleRoleBase role)
        {
            var assassin = role as Assassin;
            var jackal = role as Jackal;

            return assassin != null || jackal != null;
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
            byte roleId, byte playerId, byte id, byte bytedRoleType)
        {
            RoleTypes roleType = (RoleTypes)bytedRoleType;
            bool hasVanilaRole = roleType != RoleTypes.Crewmate || roleType != RoleTypes.Impostor;

            foreach (var combRole in CombRole)
            {

                var role = combRole.GetRole(
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
                    break;
                }
            }
        }
        public static void SetPlyerIdToSingleRoleId(
            byte roleId, byte playerId)
        {
            foreach (RoleTypes vanilaRole in Enum.GetValues(
                typeof(RoleTypes)))
            {
                if ((byte)vanilaRole == roleId)
                {
                    setPlyerIdToSingleRole(
                        playerId, new Solo.VanillaRoleWrapper(vanilaRole));
                    return;
                }
            }

            foreach (var role in NormalRole)
            {
                if (role.BytedRoleId == roleId)
                {
                    setPlyerIdToSingleRole(playerId, role);
                    return;
                }
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

        private static void createOptions(
            int optionIdOffsetChord,
            IEnumerable<RoleOptionBase> roles)
        {
            if (roles.Count() == 0) { return; };

            int roleOptionOffset = 0;

            foreach (var item
             in roles.Select((Value, Index) => new { Value, Index }))
            {
                roleOptionOffset = optionIdOffsetChord + (OptionOffsetPerRole * item.Index);
                item.Value.CreateRoleAllOption(roleOptionOffset);
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

                if (abilityRole != null)
                {
                    multiAssignAbilityRole.Button.PositionOffset = new UnityEngine.Vector3(0, 2.6f, 0);
                    multiAssignAbilityRole.Button.ReplaceHotKey(UnityEngine.KeyCode.G);
                }
            }
            Helper.Logging.Debug($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
        }

    }
}
