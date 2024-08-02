using ExtremeRoles.Helper;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class AllRoleInfoModel : PanelPageModelBase
{
	protected override void CreateAllRoleText()
	{
		IOption option;
		string colorRoleName;
		string roleFullDesc;

		foreach (var role in Roles.ExtremeRoleManager.NormalRole.Values)
		{
			colorRoleName = role.GetColoredRoleName(true);
			option = role.Loader.Get(RoleCommonOption.SpawnRate);

			roleFullDesc = Tr.GetString($"{role.Id}FullDescription");
			roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

			AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
		}

		foreach (var combRole in Roles.ExtremeRoleManager.CombRole.Values)
		{
			option = combRole.Loader.Get(RoleCommonOption.SpawnRate);

			if (combRole is ConstCombinationRoleManagerBase)
			{
				foreach (var role in combRole.Roles)
				{
					colorRoleName = role.GetColoredRoleName(true);

					roleFullDesc = Tr.GetString($"{role.Id}FullDescription");
					roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

					AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
				}
			}
			else if (combRole is FlexibleCombinationRoleManagerBase flexCombRole)
			{
				colorRoleName = flexCombRole.GetOptionName();

				roleFullDesc = flexCombRole.GetBaseRoleFullDescription();
				roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

				AddPage(new RoleInfo(colorRoleName, roleFullDesc, option));
			}
		}
	}
}
