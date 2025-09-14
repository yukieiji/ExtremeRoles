using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class AllGhostRoleInfoModel : PanelPageModelBase
{
	private readonly IEnumerable<ExtremeGhostRoleId> ids =
		ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGhostRoleInfoContainer>().Core.Keys;
	private readonly IGhostRoleProvider provider = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGhostRoleProvider>();
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

				roleFullDesc = Tr.GetString($"{role.Core.Id}FullDescription");
				roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

				AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
			}
		}


		foreach (var roleId in ids)
		{
			var role = this.provider.Get(roleId);
			option = role.Loader.Get(RoleCommonOption.SpawnRate);
			colorRoleName = role.GetColoredRoleName();

			roleFullDesc = Tr.GetString($"{role.Core.Id}FullDescription");
			roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

			AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
		}
	}
}
