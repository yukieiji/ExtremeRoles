using HarmonyLib;

using Microsoft.Extensions.DependencyInjection;

using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class KeyboardJoystickPatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null)
        { return; }

        if (ExtremeGameModeManager.Instance.RoleSelector.CanUseXion &&
            OptionManager.Instance.TryGetCategory(
				OptionTab.GeneralTab,
				ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IRoleParentOptionIdGenerator>().Get(ExtremeRoleId.Xion),
				out var cate) &&
			cate.Loader.TryGetValueOption<XionOption, bool>(XionOption.UseXion, out var opt) &&
			opt.Value &&
            !ExtremeRolesPlugin.DebugMode.Value)
        {
            Roles.Solo.Host.Xion.SpecialKeyShortCut();
        }

		InfoOverlay.Instance.Update();

        // キルとベントボタン
        if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

        var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (role.IsImpostor())
		{
			return;
		}

        var player = KeyboardJoystick.player;
        var hudManager = HudManager.Instance;

        if (player.GetButtonDown(8) && role.CanKill())
        {
            hudManager.KillButton.DoClick();
        }

        if (player.GetButtonDown(50) && role.CanUseVent())
        {
            if (role.TryGetVanillaRoleId(out RoleTypes roleId))
            {
                if (roleId != RoleTypes.Engineer ||
                    ExtremeGameModeManager.Instance.ShipOption.Vent.EngineerUseImpostorVent)
                {
                    hudManager.ImpostorVentButton.DoClick();
                }
            }
            else
            {
                hudManager.ImpostorVentButton.DoClick();
            }
        }
    }
}
