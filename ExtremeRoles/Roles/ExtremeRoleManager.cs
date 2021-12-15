using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeRoles.Roles
{
    public enum ExtremeRoleId
    {
        Null = -100,
        VanillaRole = 50,
        NormalCrew = 100,
        Assassin,
        Marlin,
    }
    public static class ExtremeRoleManager
    {
        public const int OptionOffsetPerRole = 20;

        public static readonly List<
            SingleRoleAbs> NormalRole = new List<SingleRoleAbs>()
        { 
            new Solo.Crewmate.TestCrew(),
        };
        
        public static readonly List<
            CombinationRoleManagerBase> CombRole = new List<CombinationRoleManagerBase>()
        {
            new Combination.Avalon(),
        };

        public static Dictionary<
            byte, SingleRoleAbs> GameRole = new Dictionary<byte, SingleRoleAbs> ();

        public static void CreateCombinationRoleOptions(
            int optionIdOffsetChord)
        {
            CreateOptions(optionIdOffsetChord, CombRole);
        }

        public static void CreateNormalRoleOptions(
            int optionIdOffsetChord)
        {
            CreateOptions(optionIdOffsetChord, NormalRole);
        }

        private static void CreateOptions(
            int optionIdOffsetChord,
            IEnumerable<RoleAbs> roles)
        {
            if (roles.Count() == 0) {  return; };

            int roleOptionOffset = 0;

            foreach (var item
             in roles.Select((Value, Index) => new { Value, Index }))
            {
                roleOptionOffset = optionIdOffsetChord + (OptionOffsetPerRole * item.Index);
                item.Value.CreateRoleAllOption(roleOptionOffset);
            }
        }

        public static void GameInit()
        {
            GameRole.Clear();
            foreach (var role in CombRole)
            {
                role.GameInit();
            }
        }
        public static void SetPlyerIdToRoleId(
            byte roleId, byte playerId)
        {
            foreach (RoleTypes vanilaRole in Enum.GetValues(typeof(RoleTypes)))
            {
                if ((byte)vanilaRole == roleId)
                {
                    SetPlyerIdToRole(
                        playerId, new Solo.VanillaRoleWrapper(vanilaRole));
                    return;
                }
            }

            foreach (var role in NormalRole)
            {
                if (TrySetRole(roleId, playerId, role)) { return; }
            }


            foreach (var combRole in CombRole)
            {
                foreach (var role in combRole.Roles)
                {
                    if (TrySetRole(roleId, playerId, role)) { return; }
                }
            }
        }

        private static void SetPlyerIdToRole(
            byte playerId, SingleRoleAbs role)
        {
            SingleRoleAbs addRole = role.Clone();
            addRole.GameInit();

            if (!GameRole.ContainsKey(playerId))
            {
                GameRole.Add(
                    playerId, addRole);
            }
            else
            {
                ((MultiAssignRoleAbs)GameRole[
                    playerId]).SetAnotherRole(addRole);
            }
            Modules.Helpers.DebugLog($"PlayerId:{playerId}   AssignTo:{addRole.RoleName}");
        }

        private static bool TrySetRole(
            byte roleId, byte playerId,
            SingleRoleAbs role)
        {
            if (role.BytedRoleId == roleId)
            {
                SetPlyerIdToRole(playerId, role);
                return true;
            }

            return false;
        }
        
    }
}
