using HarmonyLib;

using ExtremeRoles.GameMode;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Neutral.Yandere;

#nullable enable

namespace ExtremeRoles.Patches.Button;

[HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
public static class KillButtonDoClickPatch
{
	public enum KillResult : byte
	{
		PreConditionFail,
		BlockedToKillerSingleRoleCondition,
		BlockedToTargetSingleRoleCondition,
		BlockedToKillerOtherRoleCondition,
		BlockedToTargetOtherRoleCondition,
		BlockedToBodyguard,
		BlockedToSurrogator,
		FinalConditionFail,
		Success,
	}

    public static bool Prefix(KillButton __instance)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

        PlayerControl killer = PlayerControl.LocalPlayer;
        var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (__instance.enabled &&
            !__instance.isCoolingDown &&
            !killer.Data.IsDead &&
            killer.CanMove &&
            role.CanKill())
        {
			var target = __instance.currentTarget;
			if (!CheckPreKillConditionWithBool(role, killer, target))
			{
				return false;
			}

			var lastWolf = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<LastWolf>();

            excuteKill(
                __instance, killer, target,
                lastWolf == null || !lastWolf.IsAwake);
        }
        return false;
    }

	public static bool CheckPreKillConditionWithBool(
		SingleRoleBase killerRole,
		PlayerControl killer,
		PlayerControl? target)
		=> CheckPreKillCondition(killerRole, killer, target) is KillResult.Success;

	public static KillResult CheckPreKillCondition(
		SingleRoleBase killerRole,
		PlayerControl killer,
		PlayerControl? target)
	{
		if (killer == null ||
			target == null ||
			killer.Data == null ||
			target.Data == null ||
			target.Data.IsDead ||
			!ExtremeRoleManager.TryGetRole(target.PlayerId, out var targetRole))
		{
			return KillResult.PreConditionFail;
		}
		else if (isPreventKillTo(killerRole, killer, target))
		{
			return KillResult.BlockedToKillerSingleRoleCondition;
		}
		else if (
			targetRole.AbilityClass is IKilledFrom targetKillFromCheckRole &&
			!targetKillFromCheckRole.TryKilledFrom(target, killer))
		{
			return KillResult.BlockedToTargetSingleRoleCondition;
		}
		else if (
			killerRole is MultiAssignRoleBase killerMultiAssignRole &&
			isPreventKillTo(killerMultiAssignRole.AnotherRole, killer, target))
		{
			return KillResult.BlockedToKillerOtherRoleCondition;
		}
		else if (targetRole is MultiAssignRoleBase targetMultiAssignRole &&
			targetMultiAssignRole.AnotherRole?.AbilityClass is IKilledFrom targetMultiKilledFromCheckRole &&
			!targetMultiKilledFromCheckRole.TryKilledFrom(target, killer))
		{
			return KillResult.BlockedToTargetOtherRoleCondition;
		}
		else if (BodyGuard.TryRpcKillGuardedBodyGuard(killer.PlayerId, target.PlayerId))
		{
			return KillResult.BlockedToBodyguard;
		}
		else if (SurrogatorRole.TryGurdOnesideLover(killer, target.PlayerId))
		{
			return KillResult.BlockedToSurrogator;
		}
		else if (IsMissMurderKill(killer, target))
		{
			return KillResult.FinalConditionFail;
		}
		else
		{
			return KillResult.Success;
		}
	}

    public static bool IsMissMurderKill(
        PlayerControl killer,
        PlayerControl target)
    {
        return
            AmongUsClient.Instance.IsGameOver ||
            killer == null ||
            killer.Data == null ||
            killer.Data.IsDead ||
            killer.Data.Disconnected ||
            target == null ||
            target.Data == null ||
            target.Data.IsDead ||
            target.Data.Disconnected ||
            target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() ||
            (
                target.MyPhysics.Animations.IsPlayingEnterVentAnimation() &&
                ExtremeGameModeManager.Instance.ShipOption.Vent.CanKillVentInPlayer
            ) ||
            target.inMovingPlat ||
            MeetingHud.Instance;
    }

    private static void villainSpecialKill(
        KillButton instance,
        PlayerControl killer,
        PlayerControl target,
        SingleRoleBase targetRole)
    {
		var id = targetRole.Core.Id;
		if (id == ExtremeRoleId.Vigilante)
        {
            var vigilante = (Vigilante)targetRole;
            if (vigilante.Condition != Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip)
            {
                return;
            }
        }
        else if (id == ExtremeRoleId.Hero)
        {
            HeroAcademia.RpcDrawHeroAndVillan(
                target, killer);
            return;
        }
        else if (IsMissMurderKill(killer, target))
        {
            return;
        }
        excuteKill(instance, killer, target);
    }

    private static void excuteKill(
        KillButton instance,
        PlayerControl killer,
        PlayerControl target,
        bool isAnime = true)
    {

        Helper.Player.RpcUncheckMurderPlayer(
            killer.PlayerId, target.PlayerId,
            isAnime ? byte.MaxValue : byte.MinValue);
        instance.SetTarget(null);
    }

	private static bool isPreventKillTo(SingleRoleBase? role, PlayerControl killer, PlayerControl target)
		=> 
			role is ITryKillTo killerTryKillTo &&
			!killerTryKillTo.TryRolePlayerKillTo(killer, target);
}
