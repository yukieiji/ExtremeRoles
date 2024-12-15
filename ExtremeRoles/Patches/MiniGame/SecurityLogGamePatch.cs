using HarmonyLib;

using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Module.CustomMonoBehaviour.WithAction;
using ExtremeRoles.Extension.Il2Cpp;

namespace ExtremeRoles.Patches.MiniGame;

[HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Awake))]
public static class SecurityLogGameAwakePatch
{
	public static void Postfix(SecurityLogGame __instance)
	{
		if (SecurityDummySystemManager.TryGet(out var system) &&
			system.IsActive)
		{
			system.PostfixBegin();
			var closeAct = __instance.gameObject.TryAddComponent<OnDestroyBehavior>();
			closeAct.Add(system.PostfixClose);
		}
	}
}

[HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
public static class SecurityLogGameUpdatePatch
{
    public static bool Prefix(SecurityLogGame __instance)
    {
        if (!RoleAssignState.Instance.IsRoleSetUpEnd ||
			Roles.ExtremeRoleManager.GetLocalPlayerRole().CanUseSecurity() ||
            SecurityHelper.IsAbilityUse())
		{
			// Update内でやっている処理に特殊な処理がなくサボの処理だけなのでオーバーライドしなくてよし
			return true;
		}

        __instance.EntryPool.ReclaimAll();
        __instance.SabText.text = Tr.GetString("youDonotUse");
        __instance.SabText.gameObject.SetActive(true);

        return false;
    }
    public static void Postfix(SurveillanceMinigame __instance)
    {
        SecurityHelper.PostUpdate(__instance);
    }
}
