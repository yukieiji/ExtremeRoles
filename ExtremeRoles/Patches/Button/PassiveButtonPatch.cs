using HarmonyLib;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.SystemType;

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
			obj.transform.parent == null ||
            obj.transform.parent.name == GameSystem.BottomRightButtonGroupObjectName ||
			!GameProgressSystem.IsTaskPhase)
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
