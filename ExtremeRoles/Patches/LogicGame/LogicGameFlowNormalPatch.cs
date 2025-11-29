using HarmonyLib;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;


#nullable enable

namespace ExtremeRoles.Patches.LogicGame;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
public static class LogicGameFlowNormalIsGameOverDueToDeathPatch
{
    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class LogicGameFlowNormalCheckEndCriteriaPatch
{
	private static ExtremeGameEndCheckBehavior? checker;

    public static bool Prefix(LogicGameFlowNormal __instance)
    {
        if (!GameData.Instance)
		{
			return false;
		}
        if (TutorialManager.InstanceExists)
		{
			return true;
		}

		if (HudManager.Instance.IsIntroDisplayed ||
			ExtremeRolesPlugin.ShipState.IsDisableWinCheck ||
			!GameProgressSystem.IsRoleSetUpEnd)
		{
			return false;
		}

		if (checker == null)
		{
			checker = ShipStatus.Instance.gameObject.AddComponent<ExtremeGameEndCheckBehavior>();
		}
		checker.CheckGameEnd();
		return false;
    }
}
