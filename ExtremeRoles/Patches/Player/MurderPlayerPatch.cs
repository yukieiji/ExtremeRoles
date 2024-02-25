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
			bool isGuardianAngel = CachedPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
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

		PlayerControl player = CachedPlayerControl.LocalPlayer;

		if (!target.Data.IsDead ||
			player == null) { return; }

		byte targetPlayerId = target.PlayerId;
		byte localPlayerId = player.PlayerId;
		bool isLocalPlayerDead = localPlayerId == targetPlayerId;

		// 会議中に発生したキルでキルされた人が開いてたボタンとキルされた人へ投票しようとしていたボタンを閉じる
		if (MeetingHud.Instance != null)
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

		// 挙手ボタンを消す
		if (isLocalPlayerDead &&
			ExtremeSystemTypeManager.Instance.TryGet<IRaiseHandSystem>(
				ExtremeSystemType.RaiseHandSystem, out var system) &&
			system != null)
		{
			system.RaiseHandButtonSetActive(false);
		}

		if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		ExtremeRolesPlugin.ShipState.AddDeadInfo(
			target, DeathReason.Kill, __instance);

		var role = ExtremeRoleManager.GameRole[targetPlayerId];

		if (!role.HasTask())
		{
			target.ClearTasks();
		}

		if (ExtremeRoleManager.IsDisableWinCheckRole(role))
		{
			ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
		}

		var multiAssignRole = role as MultiAssignRoleBase;

		role.RolePlayerKilledAction(
			target, __instance);
		if (multiAssignRole != null)
		{
			if (multiAssignRole.AnotherRole != null)
			{
				multiAssignRole.AnotherRole.RolePlayerKilledAction(
					target, __instance);
			}
		}

		ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);

		if (localPlayerId != targetPlayerId)
		{
			var hookRole = ExtremeRoleManager.GameRole[
				localPlayerId] as IRoleMurderPlayerHook;
			multiAssignRole = ExtremeRoleManager.GameRole[
				localPlayerId] as MultiAssignRoleBase;

			if (hookRole != null)
			{
				hookRole.HookMuderPlayer(
					__instance, target);
			}
			if (multiAssignRole != null)
			{
				hookRole = multiAssignRole.AnotherRole as IRoleMurderPlayerHook;
				if (hookRole != null)
				{
					hookRole.HookMuderPlayer(
						__instance, target);
				}
			}
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
