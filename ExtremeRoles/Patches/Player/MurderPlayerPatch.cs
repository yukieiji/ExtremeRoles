using HarmonyLib;
using UnityEngine;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Patches.Manager;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;


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
		if (!GameProgressSystem.IsGameNow ||
			resultFlags.HasFlag(MurderResultFlags.FailedError) ||
			!ExtremeRoleManager.TryGetRole(__instance.PlayerId, out var role))
		{
			return true;
		}

		bool hasOtherKillCool = role.TryGetKillCool(out float killCool);

		if (role.Core.Id == ExtremeRoleId.Villain)
		{
			guardBreakKill(__instance, target, killCool);
			return false;
		}
		if (!hasOtherKillCool)
		{
			updateDeadbody(__instance, target);
			return true;
		}

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
				DataManager.Player.Stats.IncrementStat(StatID.Role_GuardianAngel_CrewmatesProtected);
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

		if (ExtremeRoleManager.GameRole.Count == 0)
		{
			return;
		}

		ExtremeRolesPlugin.ShipState.AddDeadInfo(
			target, DeathReason.Kill, __instance);

        if (!ExtremeRoleManager.TryGetRole(target.PlayerId, out var role))
        {
			return;
        }

		clearTask(role, target);

		if (ExtremeRoleManager.IsDisableWinCheckRole(role))
		{
			ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
		}

		invokeRoleKillAction(role, __instance, target);

		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		invokeRoleHookAction(isLocalPlayerDead, localRole, __instance, target);

		ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);
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
		var meeting = MeetingHud.Instance;
		bool isReset = false;
		foreach (var pva in meeting.playerStates)
		{
			// 死んだ人が投票ボタンを開いている場合、それをすべて閉じる
			if (isLocalPlayerDead && pva.Buttons.activeSelf)
			{
				pva.Cancel();
			}
			
			// 死んだ人のPVAに対しての処理
			if (pva.TargetPlayerId == targetPlayerId)
			{
				pva.Cancel(); // 生きている人目線 : 死んだ人の投票ボタンを強制的に閉じる
				pva.UnsetVote(); // 死んだ人目線 : 死んだ人の投票自体を消す
				if (pva.ThumbsDown != null)
				{
					pva.ThumbsDown.enabled = false;
				}
			}

			// 死んだ人に投票していた場合、その投票をキャンセル
			if (pva.VotedFor == targetPlayerId)
			{
				pva.UnsetVote();
				isReset = true;
			}
		}
		if (isReset)
		{
			meeting.ClearVote();
		}
	}

	private static void hideRaiseHandButton(in bool isLocalPlayerDead)
	{
		if (isLocalPlayerDead &&
			ExtremeSystemTypeManager.Instance.TryGet<RaiseHandSystem>(
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
		if (isLocalPlayerDead)
		{
			return;
		}

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

		DebugAnalytics.Instance.Analytics.Kill(target.Data, instance.Data);
		var stats = DataManager.Player.Stats;

		if (instance.AmOwner)
		{
			if (GameManager.Instance.IsHideAndSeek())
			{
				stats.IncrementStat(StatID.HideAndSeek_ImpostorKills);
			}
			else
			{
				stats.IncrementStat(StatID.ImpostorKills);
			}
			if (instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
			{
				stats.IncrementStat(StatID.Role_Shapeshifter_ShiftedKills);
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(
					instance.KillSfx, false, 0.8f, null);
			}
			instance.SetKillTimer(killCool);
		}

		UnityTelemetry.Instance.WriteMurder();

		target.gameObject.layer = LayerMask.NameToLayer("Ghost");
		if (target.AmOwner)
		{
			stats.IncrementStat(StatID.TimesMurdered);
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
			HudManager.Instance.KillOverlay.ShowKillAnimation(
				instance.Data, target.Data);
			target.cosmetics.SetNameMask(false);
			target.RpcSetScanner(false);
		}

		AchievementManager.Instance.OnMurder(
			instance.AmOwner, target.AmOwner,
			instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
			instance.shapeshiftTargetPlayerId, target.PlayerId);

		updateDeadbody(instance, target);
		var killAnimation = instance.KillAnimations;
		var useKillAnimation = killAnimation[
			RandomGenerator.Instance.Next(0, killAnimation.Count)];

		instance.MyPhysics.StartCoroutine(
			useKillAnimation.CoPerformKill(instance, target));

		instance.logger.Debug(
			string.Format("{0} succeeded in murdering {1}", instance.PlayerId, target.PlayerId), null);
	}

	// バイパー自決時にバイパーでキルした死体であるとわかってしまう => 自決時、強制的にデフォルトの死体を使うようにする
	private static void updateDeadbody(
		PlayerControl instance,
		PlayerControl target)
	{
		NormalGameManagerGetDeadBodyPatch.ForceDefault =
			instance.Data.Role.Role == RoleTypes.Viper &&
			instance.PlayerId == target.PlayerId &&
			ExtremeGameModeManager.Instance.CurrentGameMode is GameModes.Normal or GameModes.NormalFools;
	}
}
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
