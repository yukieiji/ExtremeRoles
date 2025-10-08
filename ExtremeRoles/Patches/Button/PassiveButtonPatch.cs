using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Patches.Button;

[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
public static class PassiveButtonReceiveClickDownPatch
{
    public static bool Prefix(PassiveButton __instance)
    {
        var obj = __instance.gameObject;
		var localPlayer = PlayerControl.LocalPlayer;

		// ゲームが開始されていない、いわゆるプレイヤーがいないとかマップがない時にボタンが押せるようにするため
        if (obj == null ||
			localPlayer == null ||
			obj.transform.parent == null ||
            obj.transform.parent.name == GameSystem.BottomRightButtonGroupObjectName ||
			ShipStatus.Instance == null ||
            ExtremeRoleManager.GameRole.Count == 0 ||
            !RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		if (localPlayer.gameObject.TryGetComponent<BoxerButtobiBehaviour>(out _))
		{
			return false;
		}

		return ExtremeRoleManager.GetLocalRoleCastedStatusFlag<IUsableOverrideStatus>(x => x.EnableUseButton);
    }
}
