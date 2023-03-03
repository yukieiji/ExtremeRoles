using AmongUs.GameOptions;
using ExtremeRoles.Roles.Solo;
using System.Runtime.CompilerServices;

namespace ExtremeRoles.Roles.API.Extension.State
{
    public static class RoleState
    {
        private static float killCoolOffset = 0.0f;

        public static void Reset()
        {
            killCoolOffset = 0.0f;
        }

        public static void AddKillCoolOffset(float offset)
        {
            killCoolOffset = killCoolOffset + offset;
        }

        public static bool TryGetVisionMod(this SingleRoleBase role,
            out float vision, out bool isApplyEnvironmentVision)
        {
            vision = role.Vision;
            isApplyEnvironmentVision = role.IsApplyEnvironmentVision;
            bool isHasOterVision = role.HasOtherVision;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return isHasOterVision; 
            }
            else
            {
                float otherVision = multiAssignRole.AnotherRole.Vision;
                vision = vision > otherVision ? vision : otherVision;
                isApplyEnvironmentVision = role.IsApplyEnvironmentVision || multiAssignRole.AnotherRole.IsApplyEnvironmentVision;
                return isHasOterVision || multiAssignRole.AnotherRole.HasOtherVision;
            }
        }

        public static bool TryGetKillCool(this SingleRoleBase role, out float killCoolTime)
        {
            killCoolTime = role.KillCoolTime;
            bool hasOtherKillCool = role.HasOtherKillCool;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                if (killCoolOffset != 0.0f)
                {
                    killCoolTime = killCoolTime + killCoolOffset;
                }
                return hasOtherKillCool;
            }
            else
            {
                float otherKillCoolTime = multiAssignRole.AnotherRole.KillCoolTime;
                killCoolTime = killCoolTime < otherKillCoolTime ? killCoolTime : otherKillCoolTime;
                if (killCoolOffset != 0.0f)
                {
                    killCoolTime = killCoolTime + killCoolOffset;
                }
                return hasOtherKillCool || multiAssignRole.AnotherRole.HasOtherKillCool;
            }
        }

        public static bool TryGetKillRange(this SingleRoleBase role, out int killRange)
        {
            killRange = role.KillRange;
            bool hasOtherKillRange = role.HasOtherKillRange;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return hasOtherKillRange;
            }
            else
            {
                int otherKillRange = multiAssignRole.AnotherRole.KillRange;
                killRange = killRange > otherKillRange ? killRange : otherKillRange;
                return hasOtherKillRange || multiAssignRole.AnotherRole.HasOtherKillRange;
            }
        }


        public static bool TryGetVelocity(this SingleRoleBase role, out float velocity)
        {
            velocity = role.MoveSpeed;
            bool isBoost = role.IsBoost;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return isBoost;
            }
            else
            {
                float otherVelocity = multiAssignRole.AnotherRole.MoveSpeed;
                velocity = velocity > otherVelocity ? velocity : otherVelocity;
                return isBoost || multiAssignRole.AnotherRole.IsBoost;
            }
        }

        public static bool CanKill(this SingleRoleBase role)
        {
            bool canKill = role.CanKill;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return canKill;
            }
            else
            {
                return canKill || multiAssignRole.AnotherRole.CanKill;
            }
        }

        public static bool CanUseVent(this SingleRoleBase role)
        {
            bool canUseVent = role.UseVent;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return canUseVent;
            }
            else
            {
                return canUseVent || multiAssignRole.AnotherRole.UseVent;
            }
        }

        public static bool HasTask(this SingleRoleBase role)
        {
            bool hasTask = role.HasTask;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return hasTask;
            }
            else
            {
                return hasTask || multiAssignRole.AnotherRole.HasTask;
            }
        }

        public static bool CanUseSabotage(this SingleRoleBase role)
        {
            bool useSabotage = role.UseSabotage;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return useSabotage;
            }
            else
            {
                return useSabotage || multiAssignRole.AnotherRole.UseSabotage;
            }
        }

        public static bool CanUseAdmin(this SingleRoleBase role)
        {
            bool useSabotage = role.CanUseAdmin;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return useSabotage;
            }
            else
            {
                return useSabotage || multiAssignRole.AnotherRole.CanUseAdmin;
            }
        }

        public static bool CanUseSecurity(this SingleRoleBase role)
        {
            bool canUseSecurity = role.CanUseSecurity;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return canUseSecurity;
            }
            else
            {
                return canUseSecurity || multiAssignRole.AnotherRole.CanUseSecurity;
            }
        }

        public static bool CanUseVital(this SingleRoleBase role)
        {
            bool canUseVital = role.CanUseVital;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return canUseVital;
            }
            else
            {
                return canUseVital || multiAssignRole.AnotherRole.CanUseVital;
            }
        }

        public static bool CanCallMeeting(this SingleRoleBase role)
        {
            bool canCallMeeting = role.CanCallMeeting;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return canCallMeeting;
            }
            else
            {
                return canCallMeeting || multiAssignRole.AnotherRole.CanCallMeeting;
            }
        }

        public static bool CanRepairSabotage(this SingleRoleBase role)
        {
            bool canRepairSabotage = role.CanRepairSabotage;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return canRepairSabotage;
            }
            else
            {
                return canRepairSabotage || multiAssignRole.AnotherRole.CanRepairSabotage;
            }
        }

        public static bool IsAssignGhostRole(this SingleRoleBase role)
        {
            bool isAssignGhostRole = role.IsAssignGhostRole;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return isAssignGhostRole;
            }
            else
            {
                return isAssignGhostRole && multiAssignRole.AnotherRole.IsAssignGhostRole;
            }
        }

        public static bool IsContainVanillaRole(this SingleRoleBase role)
        {
            if (role.IsVanillaRole())
            {
                return true;
            }
            else if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return false;
            }
            else
            {
                return multiAssignRole.AnotherRole.IsVanillaRole();
            }
        }

        public static bool TryGetVanillaRoleId(this SingleRoleBase role, out RoleTypes roleId)
        {
            if (role is VanillaRoleWrapper vanillaRole)
            {
                roleId = vanillaRole.VanilaRoleId;
                return true;
            }
            else if (
                role is MultiAssignRoleBase multiAssignRole &&
                multiAssignRole.AnotherRole is VanillaRoleWrapper anotherVanillaRole)
            {
                roleId = anotherVanillaRole.VanilaRoleId;
                return true;
            }
            else
            {
                roleId = RoleTypes.Crewmate;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool isNotMultiAssign(SingleRoleBase role, out MultiAssignRoleBase multiAssignRole)
        {
            multiAssignRole = role as MultiAssignRoleBase;
            return
                multiAssignRole == null ||
                multiAssignRole.AnotherRole == null;
        }
    }
}
