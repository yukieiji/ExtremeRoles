using ExtremeRoles.Helper;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class AllRoleInfoModel : PanelPageModelBase
{
				public override string Title => Translation.GetString("roleDesc");

				protected override void CreateAllRoleText()
				{
								int optionId;
								string colorRoleName;
								string roleFullDesc;

								foreach (var role in Roles.ExtremeRoleManager.NormalRole.Values)
								{
												optionId = role.GetRoleOptionOffset();
												colorRoleName = role.GetColoredRoleName(true);

												roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
												roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

												AddPage(new RoleInfo(colorRoleName, roleFullDesc, optionId));
								}

								foreach (var combRole in Roles.ExtremeRoleManager.CombRole.Values)
								{
												if (combRole is ConstCombinationRoleManagerBase)
												{
																foreach (var role in combRole.Roles)
																{
																				optionId = role.GetManagerOptionOffset();
																				colorRoleName = role.GetColoredRoleName(true);

																				roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
																				roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

																				AddPage(new RoleInfo(colorRoleName, roleFullDesc, optionId));
																}
												}
												else if (combRole is FlexibleCombinationRoleManagerBase flexCombRole)
												{
																optionId = flexCombRole.GetOptionIdOffset();
																colorRoleName = flexCombRole.GetOptionName();

																roleFullDesc = flexCombRole.GetBaseRoleFullDescription();
																roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

																AddPage(new RoleInfo(colorRoleName, roleFullDesc, optionId));
												}
								}
				}
}
