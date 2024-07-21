using Assets.CoreScripts;

using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Patches.Player;

#nullable enable

#pragma warning disable Harmony003

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControlMurderPlayerPatch
{
	public static bool Prefix(
		PlayerControl __instance,
		[HarmonyArgument(0)] PlayerControl target,
		[HarmonyArgument(1)] MurderResultFlags resultFlags)
	{
		if (ExtremeRoleManager.GameRole.Count == 0 ||
			resultFlags.HasFlag(MurderResultFlags.FailedError)) { return true; }

		var role = ExtremeRoleManager.GameRole[__instance.PlayerId];

		bool hasOtherKillCool = role.TryGetKillCool(out float killCool);

		if (role.Id == ExtremeRoleId.Villain)
		{
			guardBreakKill(__instance, target, killCool);
			return false;
		}
		if (!hasOtherKillCool) { return true; }

		__instance.logger.Debug(
			$"{__instance.PlayerId} trying to murder {target.PlayerId}", null);

		bool hasDecisionByHost = resultFlags.HasFlag(MurderResultFlags.DecisionByHost);

		if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
			(
				hasDecisionByHost &&
				target.protectedByGuardianId > -1
			))
		{
			target.protectedByGuardianThisRound = true;
			bool isGuardianAngel = PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
			if (__instance.AmOwner || isGuardianAngel)
			{
				target.ShowFailedMurder();
				__instance.SetKillTimer(killCool / 2f);
			}
			else
			{
				target.RemoveProtection();
			}
			if (isGuardianAngel)
			{
				StatsManager.Instance.IncrementStat(
					StringNames.StatsGuardianAngelCrewmatesProtected);
			}
			__instance.logger.Debug(
				$"{__instance.PlayerId} failed to murder {target.PlayerId} due to guardian angel protection",
				null);
			return false;
		}

		if (resultFlags.HasFlag(MurderResultFlags.Succeeded) || hasDecisionByHost)
		{
			murderPlayerBody(__instance, target, killCool);
		}
		return false;
	}

	public static void Postfix(
		PlayerControl __instance,
		[HarmonyArgument(0)] PlayerControl target)
	{

		PlayerControl player = PlayerControl.LocalPlayer;

		if (!target.Data.IsDead ||
			player == null) { return; }

		byte targetPlayerId = target.PlayerId;
		byte localPlayerId = player.PlayerId;
		bool isLocalPlayerDead = localPlayerId == targetPlayerId;

		if (MeetingHud.Instance != null)
		{
			hidePlayerVoteAreaButton(isLocalPlayerDead, targetPlayerId);
			hideRaiseHandButton(isLocalPlayerDead);
		}

		if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		ExtremeRolesPlugin.ShipState.AddDeadInfo(
			target, DeathReason.Kill, __instance);

		var role = ExtremeRoleManager.GameRole[targetPlayerId];

		clearTask(role, target);

		if (ExtremeRoleManager.IsDisableWinCheckRole(role))
		{
			ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
		}

		invokeRoleKillAction(role, __instance, target);

		ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);

		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		invokeRoleHookAction(isLocalPlayerDead, localRole, __instance, target);
	}

	private static void clearTask(in SingleRoleBase role, in PlayerControl target)
	{
		if (!role.HasTask())
		{
			target.ClearTasks();
		}
	}

	private static void guardBreakKill(
		PlayerControl instance,
		PlayerControl target,
		float killCool)
	{
		if (target.protectedByGuardianId > -1)
		{
			target.RemoveProtection();
		}
		murderPlayerBody(instance, target, killCool);
	}

	private static void hidePlayerVoteAreaButton(
		in bool isLocalPlayerDead, in byte targetPlayerId)
	{
		foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
		{
			if ((
					isLocalPlayerDead &&
					pva.Buttons.activeSelf
				) ||
				pva.TargetPlayerId == targetPlayerId)
			{
				pva.Cancel();
			}
		}
	}

	private static void hideRaiseHandButton(in bool isLocalPlayerDead)
	{
		if (isLocalPlayerDead &&
			ExtremeSystemTypeManager.Instance.TryGet<IRaiseHandSystem>(
				ExtremeSystemType.RaiseHandSystem, out var system))
		{
			system.RaiseHandButtonSetActive(false);
		}
	}

	private static void invokeRoleKillAction(
		in SingleRoleBase role,
		in PlayerControl killer,
		in PlayerControl target)
	{
		role.RolePlayerKilledAction(target, killer);
		if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is not null)
		{
			multiAssignRole.AnotherRole.RolePlayerKilledAction(
				target, killer);
		}
	}

	private static void invokeRoleHookAction(
		in bool isLocalPlayerDead,
		in SingleRoleBase localPlayerRole,
		in PlayerControl killer,
		in PlayerControl target)
	{
		if (isLocalPlayerDead) { return; }

		if (localPlayerRole is IRoleMurderPlayerHook hookRole)
		{
			hookRole.HookMuderPlayer(killer, target);
		}
		if (localPlayerRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleMurderPlayerHook multiHookRole)
		{
			multiHookRole.HookMuderPlayer(killer, target);
		}
	}

	private static void murderPlayerBody(
		PlayerControl instance,
		PlayerControl target,
		float killCool)
	{

		FastDestroyableSingleton<DebugAnalytics>.Instance.Analytics.Kill(target.Data, instance.Data);
		var statsMng = StatsManager.Instance;

		if (instance.AmOwner)
		{
			if (GameManager.Instance.IsHideAndSeek())
			{
				statsMng.IncrementStat(
					StringNames.StatsImpostorKills_HideAndSeek);
			}
			else
			{
				statsMng.IncrementStat(StringNames.StatsImpostorKills);
			}
			if (instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
			{
				statsMng.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(
					instance.KillSfx, false, 0.8f, null);
			}
			instance.SetKillTimer(killCool);
		}

		FastDestroyableSingleton<UnityTelemetry>.Instance.WriteMurder();

		target.gameObject.layer = LayerMask.NameToLayer("Ghost");
		if (target.AmOwner)
		{
			statsMng.IncrementStat(StringNames.StatsTimesMurdered);
			if (Minigame.Instance)
			{
				try
				{
					Minigame.Instance.Close();
					Minigame.Instance.Close();
				}
				catch
				{
				}
			}
			FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
				instance.Data, target.Data);
			target.cosmetics.SetNameMask(false);
			target.RpcSetScanner(false);
		}

		FastDestroyableSingleton<AchievementManager>.Instance.OnMurder(
			instance.AmOwner, target.AmOwner,
			instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
			instance.shapeshiftTargetPlayerId, target.PlayerId);

		var killAnimation = instance.KillAnimations;
		var useKillAnimation = killAnimation[
			RandomGenerator.Instance.Next(0, killAnimation.Count)];
		instance.MyPhysics.StartCoroutine(
			useKillAnimation.CoPerformKill(instance, target));

		instance.logger.Debug(
			string.Format("{0} succeeded in murdering {1}", instance.PlayerId, target.PlayerId), null);
	}
}
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
