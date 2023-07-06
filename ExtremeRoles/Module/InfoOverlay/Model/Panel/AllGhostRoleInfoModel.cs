using System.Collections.Generic;

using ExtremeRoles.Helper;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class AllGhostRoleInfoModel : PanelPageModelBase
{
				public override string Info => Translation.GetString("changeGhostRoleMore");

				public override string Title => Translation.GetString("ghostRoleDesc");

				protected override void CreateAllRoleText()
				{
								int optionId;
								string colorRoleName;
								string roleFullDesc;

								foreach (var combRole in Roles.ExtremeRoleManager.CombRole.Values)
								{
												var ghostCombRole = combRole as GhostAndAliveCombinationRoleManagerBase;

												if (ghostCombRole == null) { continue; }

												foreach (var role in ghostCombRole.CombGhostRole.Values)
												{
																optionId = ghostCombRole.GetOptionIdOffset();
																colorRoleName = role.GetColoredRoleName();

																roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
																roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

																AddPage(new RoleInfo(colorRoleName, roleFullDesc, optionId));
												}
								}


								foreach (var role in GhostRoles.ExtremeGhostRoleManager.AllGhostRole.Values)
								{
												optionId = role.OptionOffset;
												colorRoleName = role.GetColoredRoleName();

												roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
												roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

												AddPage(new RoleInfo(colorRoleName, roleFullDesc, optionId));
								}
				}
}
