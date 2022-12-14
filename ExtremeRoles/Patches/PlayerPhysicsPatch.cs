using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches
{
    [HarmonyPatch(typeof(PlayerPhysics), "TrueSpeed", MethodType.Getter)]
    public static class PlayerPhysicsTrueSpeedPatch
    {
        // もしもっと高速で動くやつを実装する場合ここを変える
        // 正直9倍速でもカメラ追いつかねぇ・・・
        // 2022/08/14:Xionは最大20倍速で動けるのでとりあえず100を突っ込んどく
        private const float maxModSpeed = 100.0f;

        private static float playerBaseSpeed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.PlayerSpeedMod);

        public static bool Prefix(
            PlayerPhysics __instance,
            ref float __result)
        {
            // オバロとかでも以下が最大速度なのでそれを返す
            // 最大速度 = 基本速度 * PlayerControl.GameOptions.PlayerSpeedMod * 3.0f;
            __result = __instance.Speed * maxModSpeed * playerBaseSpeed;
            return false;
        }
    }


    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsFixedUpdatePatch
    {
        public static bool Prefix(PlayerPhysics __instance)
        {
            return 
                DestroyableSingleton<HudManager>.InstanceExists && 
                FastDestroyableSingleton<HudManager>.Instance.joystick != null;
        }

        public static void Postfix(PlayerPhysics __instance)
        {
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return; }
            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            if (__instance.AmOwner && 
                __instance.myPlayer.CanMove && 
                GameData.Instance &&
                ExtremeRoleManager.GetLocalPlayerRole().TryGetVelocity(out float velocity))
            {
                __instance.body.velocity *= velocity;
            }
        }
    }
}
