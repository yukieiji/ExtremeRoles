using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance;



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
			var option = role.Loader.Get(RoleCommonOption.SpawnRate);
			roleOptionString = IInfoOverlayPanelModel.ToHudStringWithChildren(option);
		}

		string roleFullDesc = role.GetFullDescription();

		return (
			$"<size=150%>・{colorRoleName}</size>\n{roleFullDesc}\n",
			$"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n{roleOptionString}"
		);
	}
}
