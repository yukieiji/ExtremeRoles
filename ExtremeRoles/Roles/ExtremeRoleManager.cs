using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles
{
    public enum ExtremeRoleId
    {
        Null = -100,
        VanillaRole = 50,
        Assassin,
        Marlin,
        Lover,
        Jackal,
        SideKick,
        
        Alice,
        NormalCrew = 100,
    }
    public static class ExtremeRoleManager
    {
        public const int OptionOffsetPerRole = 30;

        public static readonly List<
            SingleRoleBase> NormalRole = new List<SingleRoleBase>()
        { 
            new Solo.Neutral.Alice(),
        };
        
        public static readonly List<
            CombinationRoleManagerBase> CombRole = new List<CombinationRoleManagerBase>()
        {
            new Combination.Avalon(),
        };

        public static Dictionary<
            byte, SingleRoleBase> GameRole = new Dictionary<byte, SingleRoleBase> ();

        public static void CreateCombinationRoleOptions(
            int optionIdOffsetChord)
        {
            createOptions(optionIdOffsetChord, CombRole);
        }

        public static void CreateNormalRoleOptions(
            int optionIdOffsetChord)
        {
            createOptions(optionIdOffsetChord, NormalRole);
        }

        public static void GameInit()
        {
            GameRole.Clear();
            foreach (var role in CombRole)
            {
                role.GameInit();
            }
        }

        public static SingleRoleBase GetLocalPlayerRole()
        {
            return GameRole[PlayerControl.LocalPlayer.PlayerId];
        }

        public static void SetPlayerIdToMultiRoleId(
            byte roleId, byte playerId, byte id)
        {

            RoleTypes roleType = Helper.Player.GetPlayerControlById(playerId).Data.Role.Role;
            bool hasVanilaRole = roleType != RoleTypes.Crewmate || roleType != RoleTypes.Impostor;

            foreach (var combRole in CombRole)
            {
                foreach (var role in combRole.Roles)
                {
                    if (role.BytedRoleId == roleId)
                    {

                        SingleRoleBase addRole = role.Clone();

                        IRoleAbility abilityRole = addRole as IRoleAbility;

                        if (abilityRole != null && PlayerControl.LocalPlayer.PlayerId == playerId)
                        {
                            Helper.Logging.Debug("Try Create Ability NOW!!!");
                            abilityRole.CreateAbility();
                        }

                        addRole.GameInit();
                        ((MultiAssignRoleBase)addRole).GameId = id;

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
                }
            }
        }

        private static void createOptions(
            int optionIdOffsetChord,
            IEnumerable<RoleSettingBase> roles)
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

            addRole.GameInit();

            if (!GameRole.ContainsKey(playerId))
            {
                GameRole.Add(
                    playerId, addRole);

            }
            else
            {
                ((MultiAssignRoleBase)GameRole[
                    playerId]).SetAnotherRole(addRole);
            }
            Helper.Logging.Debug($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
        }
        
    }
}
