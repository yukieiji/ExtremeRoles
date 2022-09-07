using System.Runtime.CompilerServices;

namespace ExtremeRoles.Roles.API.Extension.State
{
    public static class RoleState
    {
        public static float KillCoolOffset = 0.0f;

        public static void Reset()
        {
            KillCoolOffset = 0.0f;
        }

        public static bool TryGetVisonMod(this SingleRoleBase role,
            out float vison, out bool isApplyEnvironmentVision)
        {
            vison = role.Vison;
            isApplyEnvironmentVision = role.IsApplyEnvironmentVision;
            bool isHasOterVison = role.HasOtherVison;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                return isHasOterVison; 
            }
            else
            {
                float otherVison = multiAssignRole.AnotherRole.Vison;
                vison = vison > otherVison ? vison : otherVison;
                isApplyEnvironmentVision = role.IsApplyEnvironmentVision || multiAssignRole.AnotherRole.IsApplyEnvironmentVision;
                return isHasOterVison || multiAssignRole.AnotherRole.HasOtherVison;
            }
        }

        public static bool TryGetKillCool(this SingleRoleBase role, out float killCoolTime)
        {
            killCoolTime = role.KillCoolTime;
            bool hasOtherKillCool = role.HasOtherKillCool;

            if (isNotMultiAssign(role, out MultiAssignRoleBase multiAssignRole))
            {
                if (KillCoolOffset != 0.0f)
                {
                    killCoolTime = killCoolTime + KillCoolOffset;
                }
                return hasOtherKillCool;
            }
            else
            {
                float otherKillCoolTime = multiAssignRole.AnotherRole.KillCoolTime;
                killCoolTime = killCoolTime < otherKillCoolTime ? killCoolTime : otherKillCoolTime;
                if (KillCoolOffset != 0.0f)
                {
                    killCoolTime = killCoolTime + KillCoolOffset;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool isNotMultiAssign(SingleRoleBase role, out MultiAssignRoleBase multiAssignRole)
        {
            multiAssignRole = role as MultiAssignRoleBase;
            return 
                multiAssignRole == null || 
                multiAssignRole.AnotherRole == null ||
                multiAssignRole.AnotherRole.Id == ExtremeRoleId.VanillaRole;
        }
    }
}
