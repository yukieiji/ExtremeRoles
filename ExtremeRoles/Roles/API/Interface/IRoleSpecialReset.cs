using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo;

#nullable enable

namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleSpecialReset
{
    public void AllReset(PlayerControl rolePlayer);

	public static void ResetLover(byte targetPlayerId)
	{
		var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (!(targetPlayer != null &&
			ExtremeRoleManager.TryGetRole(targetPlayerId, out var role)))
		{
			return;
		}
		ResetLover(role, targetPlayer);
	}

	public static void ResetLover(SingleRoleBase targetRole, PlayerControl rolePlayer)
	{
		if (targetRole.Id != ExtremeRoleId.Lover)
		{
			return;
		}
		targetRole.RolePlayerKilledAction(rolePlayer, rolePlayer);
	}

    public static void ResetRole(byte targetPlayerId)
    {
        PlayerControl resetPlayer = Player.GetPlayerControlById(targetPlayerId);
        SingleRoleBase resetRole = ExtremeRoleManager.GameRole[targetPlayerId];

        IParentChainStatus.PurgeParent(targetPlayerId);

        // プレイヤーのリセット処理
        if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
        {
            Player.ResetTarget();
            abilityReset(resetRole);
        }

        // シェイプシフターのリセット処理
        shapeshiftReset(resetPlayer, resetRole);

        // スペシャルリセット処理
        specialResetRoleReset(resetPlayer, resetRole);

        // クルーに変更
        RoleManager.Instance.SetRole(
            Player.GetPlayerControlById(targetPlayerId),
            RoleTypes.Crewmate);
    }

    private static void abilityReset(
        SingleRoleBase targetRole)
    {
        if (targetRole is IRoleAbility abilityRole)
        {
            abilityRole.Button.OnMeetingStart();
        }
        if (targetRole is IRoleResetMeeting meetingResetRole)
        {
            meetingResetRole.ResetOnMeetingStart();
        }

        if (targetRole is MultiAssignRoleBase multiAssignRole)
        {
            if (multiAssignRole.AnotherRole is IRoleAbility multiAssignAbilityRole)
            {
                multiAssignAbilityRole.Button.OnMeetingStart();
            }

            if (multiAssignRole.AnotherRole is IRoleResetMeeting multiAssignMeetingResetRole)
            {
                multiAssignMeetingResetRole.ResetOnMeetingStart();
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
        if (targetRole is IRoleSpecialReset specialResetRole)
        {
            specialResetRole.AllReset(targetPlayer);
        }

        if (targetRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleSpecialReset multiSpecialReset)
        {
			multiSpecialReset.AllReset(targetPlayer);
		}
    }
}
