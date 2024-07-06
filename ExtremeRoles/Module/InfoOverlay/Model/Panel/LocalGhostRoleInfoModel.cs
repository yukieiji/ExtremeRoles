using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API.Interface;


namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class LocalGhostRoleInfoModel : IInfoOverlayPanelModel
{
	public (string, string) GetInfoText()
	{
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			return ($"<size=200%>{Translation.GetString("yourAliveNow")}</size>\n", string.Empty);
		}

		var role = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
		if (role == null)
		{
			return ($"<size=200%>{Translation.GetString("yourNoAssignGhostRole")}</size>\n", "");
		}

		string roleOptionString = "";
		string colorRoleName = role.GetColoredRoleName();

		if (!role.IsVanillaRole())
		{
			var useLoader =
				role is ICombination combGhost &&
				combGhost.OffsetInfo is not null &&
				ExtremeRoleManager.CombRole.TryGetValue((byte)combGhost.OffsetInfo.RoleId, out var combRole) &&
					combRole is not null ?
					combRole.Loader : role.Loader;

			var option = useLoader.Get(RoleCommonOption.SpawnRate);
			roleOptionString = IInfoOverlayPanelModel.ToHudStringWithChildren(option);
		}

		string roleFullDesc = role.GetFullDescription();

		return (
			$"<size=150%>・{colorRoleName}</size>\n{roleFullDesc}\n",
			$"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n{roleOptionString}"
		);
	}
}
