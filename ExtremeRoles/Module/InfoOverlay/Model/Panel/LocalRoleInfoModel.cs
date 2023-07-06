using System;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class LocalRoleInfoModel : IInfoOverlayPanelModel
{
				public string Title => Translation.GetString("yourRole");

				private const string oneLineRoleInfoPlaceholder = "<size=150%>・{0}</size>\n{1}\n<size=115%>・{0}{3}</size>\n{4}";

				public (string, string) GetInfoText()
				{
								var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();

								if (role is MultiAssignRoleBase multiAssignRole)
								{
												return createMultiAssignRoleInfo(multiAssignRole);
								}
								else
								{
												(string colorRoleName, string roleFullDesc, string roleOptionString) = getRoleInfoAndOption(role);

												return (
																$"<size=150%>・{colorRoleName}</size>\n{roleFullDesc}",
																$"<size=115%>・{colorRoleName}{Translation.GetString("roleOption")}</size>\n{roleOptionString}"
												);
								}
				}

				private static (string, string) createMultiAssignRoleInfo(MultiAssignRoleBase multiAssignRole)
				{

								(string colorRoleName, string roleFullDesc, string roleOptionString) = getMultiRoleInfoAndOption(
												multiAssignRole);
								string roleOptionStr = Translation.GetString("roleOption");

								if (multiAssignRole.AnotherRole is not null)
								{
												(string anotherColorRoleName, string anotherRoleFullDesc, string anotherRoleOptionString) =
																getRoleInfoAndOption(multiAssignRole.AnotherRole);
												return (
																string.Format(
																				oneLineRoleInfoPlaceholder,
																				colorRoleName, roleFullDesc,
																				roleOptionString, roleOptionStr),
																string.Format(
																				oneLineRoleInfoPlaceholder,
																				anotherColorRoleName, anotherRoleFullDesc,
																				roleOptionString, anotherRoleOptionString));
								}
								else
								{
												return (
																$"<size=150%>・{colorRoleName}</size>\n{roleFullDesc}",
																$"<size=115%>・{colorRoleName}{Translation.GetString("roleOption")}</size>\n{roleOptionString}"
								);
								}
				}

				private static (string, string, string) getRoleInfoAndOption(SingleRoleBase role)
				{
								var allOption = OptionManager.Instance;

								string roleOptionString = "";

								if (!role.IsVanillaRole())
								{
												var option = allOption.GetIOption(role.GetRoleOptionId(RoleCommonOption.SpawnRate));
												roleOptionString = option.ToHudStringWithChildren();
								}
								string colorRoleName = role.GetColoredRoleName();
								string roleFullDesc = role.GetFullDescription();

								replaceAwakeRoleOptionString(ref roleOptionString, role);

								return (colorRoleName, roleFullDesc, roleOptionString);
				}

				private static (string, string, string) getMultiRoleInfoAndOption(MultiAssignRoleBase role)
				{
								var allOption = OptionManager.Instance;

								string roleOptionString = "";

								if (!role.IsVanillaRole())
								{
												var option = allOption.GetIOption(role.GetManagerOptionId(RoleCommonOption.SpawnRate));
												roleOptionString = option.ToHudStringWithChildren();
								}
								string colorRoleName = role.GetColoredRoleName();
								string roleFullDesc = role.GetFullDescription();

								replaceAwakeRoleOptionString(ref roleOptionString, role);

								return (colorRoleName, roleFullDesc, roleOptionString);
				}


				private static void replaceAwakeRoleOptionString(
								ref string roleOptionString, SingleRoleBase role)
				{
								if (role is IRoleAwake<RoleTypes> awakeFromVaniraRole &&
												!awakeFromVaniraRole.IsAwake)
								{
												roleOptionString = string.Empty;
								}
								else if (
												role is IRoleAwake<Roles.ExtremeRoleId> awakeFromExRole &&
												!awakeFromExRole.IsAwake)
								{
												roleOptionString = awakeFromExRole.GetFakeOptionString();
								}
				}
}
