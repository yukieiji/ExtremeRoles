using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable

// HotFix : 死人のペットが普通に見えるバグ修正、もうペットだけ消す
[HarmonyPatch(typeof(PetBehaviour), nameof(PetBehaviour.SetMourning))]
public static class PlayerControlDiePatch
{
	public static void Postfix(PetBehaviour __instance)
	{
		__instance.gameObject.SetActive(false);
	}
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
public static class PlayerControlRevivePatch
{
	public static void Postfix(PlayerControl __instance)
	{

		byte revivePlayerId = __instance.PlayerId;
		ExtremeRolesPlugin.ShipState.RemoveDeadInfo(revivePlayerId);

		// 消したペットをもとに戻しておく
		if (!__instance.Data.IsDead &&
			__instance.cosmetics.currentPet != null)
		{
			__instance.cosmetics.currentPet.gameObject.SetActive(true);
			__instance.cosmetics.currentPet.SetIdle();
		}

		if (ExtremeRoleManager.GameRole.Count == 0) { return; }
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

		bool isLocalPlayerRevive = revivePlayerId == CachedPlayerControl.LocalPlayer.PlayerId;

		invokeReviveAction(__instance);
		invokeReviveHook(__instance, isLocalPlayerRevive);

		RoleTypes roleId = RoleTypes.Crewmate;

		if (ExtremeRoleManager.GameRole.TryGetValue(revivePlayerId, out var role) &&
			role is not null &&
			!role.TryGetVanillaRoleId(out roleId) &&
			role.IsImpostor())
		{
			roleId = RoleTypes.Impostor;
		}

		FastDestroyableSingleton<RoleManager>.Instance.SetRole(
			__instance, roleId);

		var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
		if (ghostRole == null) { return; }

		if (isLocalPlayerRevive)
		{
			ghostRole.ResetOnMeetingStart();
		}

		lock (ExtremeGhostRoleManager.GameRole)
		{
			ExtremeGhostRoleManager.GameRole.Remove(revivePlayerId);
		}
	}

	private static void invokeReviveAction(in PlayerControl revivePlayer)
	{
		var (onRevive, onReviveOther) = ExtremeRoleManager.GetInterfaceCastedRole<
			IRoleOnRevive>(revivePlayer.PlayerId);

		onRevive?.ReviveAction(revivePlayer);
		onReviveOther?.ReviveAction(revivePlayer);
	}

	private static void invokeReviveHook(in PlayerControl revivePlayer, in bool isLocalPlayerRevive)
	{
		if (isLocalPlayerRevive)
		{
			return;
		}

		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (localRole is IRoleReviveHook hookRole)
		{
			hookRole.HookRevive(revivePlayer);
		}
		if (localRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleReviveHook multiHookRole)
		{
			multiHookRole.HookRevive(revivePlayer);
		}
	}
}
