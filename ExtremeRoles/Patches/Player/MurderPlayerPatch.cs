using Assets.CoreScripts;

using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControlMurderPlayerPatch
{
	public static bool Prefix(
		PlayerControl __instance,
		[HarmonyArgument(0)] PlayerControl target)
	{
		if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

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

		if (target.protectedByGuardian)
		{
			target.protectedByGuardianThisRound = true;
			bool flag = CachedPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
			if (__instance.AmOwner || flag)
			{
				target.ShowFailedMurder();
				__instance.SetKillTimer(killCool / 2f);
			}
			else
			{
				target.RemoveProtection();
			}
			if (flag)
			{
				StatsManager.Instance.IncrementStat(
					StringNames.StatsGuardianAngelCrewmatesProtected);
			}
			__instance.logger.Debug(
				$"{__instance.PlayerId} failed to murder {target.PlayerId} due to guardian angel protection",
				null);
			return false;
		}

		murderPlayerBody(__instance, target, killCool);
		return false;
	}

	public static void Postfix(
		PlayerControl __instance,
		[HarmonyArgument(0)] PlayerControl target)
	{

		if (ExtremeRoleManager.GameRole.Count == 0) { return; }

		if (!target.Data.IsDead) { return; }

		ExtremeRolesPlugin.ShipState.AddDeadInfo(
			target, DeathReason.Kill, __instance);

		byte targetPlayerId = target.PlayerId;

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

		var player = CachedPlayerControl.LocalPlayer;

		if (player.PlayerId != targetPlayerId)
		{
			var hookRole = ExtremeRoleManager.GameRole[
				player.PlayerId] as IRoleMurderPlayerHook;
			multiAssignRole = ExtremeRoleManager.GameRole[
				player.PlayerId] as MultiAssignRoleBase;

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
		if (target.protectedByGuardian)
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
		if (instance.AmOwner)
		{
			if (GameManager.Instance.IsHideAndSeek())
			{
				StatsManager.Instance.IncrementStat(
					StringNames.StatsImpostorKills_HideAndSeek);
			}
			else
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
			}
			if (instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
			{
				StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(
					instance.KillSfx, false, 0.8f, null);
			}
			instance.SetKillTimer(killCool);
		}

		FastDestroyableSingleton<Telemetry>.Instance.WriteMurder();

		target.gameObject.layer = LayerMask.NameToLayer("Ghost");
		if (target.AmOwner)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsTimesMurdered);
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
			FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
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
