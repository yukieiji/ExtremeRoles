using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class LocalGhostRoleInfoModel : IInfoOverlayPanelModel
{
				public string Title => Translation.GetString("yourGhostRole");

				public (string, string) GetInfoText()
				{
								if (!CachedPlayerControl.LocalPlayer.Data.IsDead)
								{
												return ($"<size=200%>{Translation.GetString("yourAliveNow")}</size>\n", string.Empty);
								}
								
								var role = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
								if (role == null)
								{
												return ($"<size=200%>{Translation.GetString("yourNoAssignGhostRole")}</size>\n", "");
								}

								var allOption = OptionManager.Instance;

								string roleOptionString = "";
								string colorRoleName = role.GetColoredRoleName();

								if (!role.IsVanillaRole())
								{
												int useId = role.GetRoleOptionId(RoleCommonOption.SpawnRate);

												if (!allOption.Contains(useId))
												{
																var aliveRole = (MultiAssignRoleBase)ExtremeRoleManager.GetLocalPlayerRole();
																useId = aliveRole.GetManagerOptionId(RoleCommonOption.SpawnRate);
												}

												var option = allOption.GetIOption(useId);
												roleOptionString = option.ToHudStringWithChildren();
								}

								string roleFullDesc = role.GetFullDescription();

								return (
												$"<size=150%>・{colorRoleName}</size>\n{roleFullDesc}\n",
												$"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n{roleOptionString}"
								);
				}
}
