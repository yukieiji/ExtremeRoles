using ExtremeRoles.Helper;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class AllGhostRoleInfoModel : RolePagePanelModelBase
{
	protected override void CreateAllRoleText()
	{
		IOption option;
		string colorRoleName;
		string roleFullDesc;

		foreach (var combRole in Roles.ExtremeRoleManager.CombRole.Values)
		{
			var ghostCombRole = combRole as GhostAndAliveCombinationRoleManagerBase;

			if (ghostCombRole == null) { continue; }

			option = combRole.Loader.Get(RoleCommonOption.SpawnRate);

			foreach (var role in ghostCombRole.CombGhostRole.Values)
			{
				colorRoleName = role.GetColoredRoleName();

				roleFullDesc = Tr.GetString($"{role.Id}FullDescription");
				roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

				AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
			}
		}


		foreach (var role in GhostRoles.ExtremeGhostRoleManager.AllGhostRole.Values)
		{
			option = role.Loader.Get(RoleCommonOption.SpawnRate);
			colorRoleName = role.GetColoredRoleName();

			roleFullDesc = Tr.GetString($"{role.Id}FullDescription");
			roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

			AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
		}
	}
}
