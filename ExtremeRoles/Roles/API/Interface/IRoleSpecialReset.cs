using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo;

namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleSpecialReset
    {
        public void AllReset(PlayerControl rolePlayer);

        public static void ResetRole(byte targetPlayerId)
        {
            PlayerControl resetPlayer = Player.GetPlayerControlById(targetPlayerId);
            SingleRoleBase resetRole = ExtremeRoleManager.GameRole[targetPlayerId];

            IRoleHasParent.PurgeParent(targetPlayerId);

            // プレイヤーのリセット処理
            if (CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId)
            {
                Player.ResetTarget();
                abilityReset(resetRole);
            }

            // シェイプシフターのリセット処理
            shapeshiftReset(resetPlayer, resetRole);

            // スペシャルリセット処理
            specialResetRoleReset(resetPlayer, resetRole);

            // クルーに変更
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(
                Player.GetPlayerControlById(targetPlayerId),
                RoleTypes.Crewmate);
        }

        private static void abilityReset(
            SingleRoleBase targetRole)
        {
            IRoleResetMeeting meetingResetRole = targetRole as IRoleResetMeeting;
            if (meetingResetRole != null)
            {
                meetingResetRole.ResetOnMeetingStart();
            }
            IRoleAbility abilityRole = targetRole as IRoleAbility;
            if (abilityRole != null)
            {
                abilityRole.ResetOnMeetingStart();
            }

            MultiAssignRoleBase multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    meetingResetRole = multiAssignRole.AnotherRole as IRoleResetMeeting;
                    if (meetingResetRole != null)
                    {
                        meetingResetRole.ResetOnMeetingStart();
                    }

                    abilityRole = multiAssignRole.AnotherRole as IRoleAbility;
                    if (abilityRole != null)
                    {
                        abilityRole.ResetOnMeetingStart();
                    }
                }
            }
        }

        private static void shapeshiftReset(
            PlayerControl targetPlayer,
            SingleRoleBase targetRole)
        {
            // シェイプシフターのリセット処理
            if (targetRole.TryGetVanillaRoleId(out RoleTypes roleId))
            {
                if (roleId == RoleTypes.Shapeshifter)
                {
                    targetPlayer.Shapeshift(targetPlayer, false);
                }
            }
        }

        private static void specialResetRoleReset(
            PlayerControl targetPlayer,
            SingleRoleBase targetRole)
        {
            IRoleSpecialReset specialResetRole = targetRole as IRoleSpecialReset;
            if (specialResetRole != null)
            {
                specialResetRole.AllReset(targetPlayer);
            }

            MultiAssignRoleBase multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    specialResetRole = multiAssignRole.AnotherRole as IRoleSpecialReset;
                    if (specialResetRole != null)
                    {
                        specialResetRole.AllReset(targetPlayer);
                    }
                }
            }
        }
    }
}
