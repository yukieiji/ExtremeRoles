using System.Text;

using Microsoft.Extensions.DependencyInjection;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class LocalRoleInfoModel : IInfoOverlayPanelModel
{
	private const string oneLineRoleInfoPlaceholder = "<size=150%>・{0}</size>\n{1}\n\n<size=115%>・{0}{2}</size>\n{3}";

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
				$"<size=115%>・{colorRoleName}{Tr.GetString("roleOption")}</size>\n{roleOptionString}"
			);
		}
	}

	private static (string, string) createMultiAssignRoleInfo(MultiAssignRoleBase multiAssignRole)
	{

		(string colorRoleName, string roleFullDesc, string roleOptionString) = getMultiRoleInfoAndOption(
			multiAssignRole);
		string settingTransStr = Tr.GetString("roleOption");

		if (multiAssignRole.AnotherRole is not null)
		{
			(string anotherColorRoleName, string anotherRoleFullDesc, string anotherRoleOptionString) =
				getRoleInfoAndOption(multiAssignRole.AnotherRole);
			return (
				string.Format(
					oneLineRoleInfoPlaceholder,
					colorRoleName, roleFullDesc, settingTransStr,
					roleOptionString),
				string.Format(
					oneLineRoleInfoPlaceholder,
					anotherColorRoleName, anotherRoleFullDesc, settingTransStr,
					anotherRoleOptionString));
		}
		else
		{
			return (
				$"<size=150%>・{colorRoleName}</size>\n{roleFullDesc}",
				$"<size=115%>・{colorRoleName}{Tr.GetString("roleOption")}</size>\n{roleOptionString}"
			);
		}
	}

	private static (string, string, string) getRoleInfoAndOption(SingleRoleBase role)
	{
		string roleOptionString = "";

		var id = role.Core.Id;
		var builder = new StringBuilder();

		// リベラル役職には全部グローバル設定を見やすいように追加しておく
		if (role.IsLiberal())
		{
			var liberalSetting = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptipnLoader>();
			foreach (var target in liberalSetting.GlobalOption)
			{
				IInfoOverlayPanelModel.AddHudStringWithChildren(builder, target);
			}
		}
		if (id is
				ExtremeRoleId.Leader or
				ExtremeRoleId.Dove or
				ExtremeRoleId.Militant)
		{
			var liberalSetting = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptipnLoader>();
			var targets = id switch
			{ 
				ExtremeRoleId.Leader => liberalSetting.LeaderOption,
				ExtremeRoleId.Militant => liberalSetting.MiltantOption,
				_ => []
			};
			foreach (var target in targets)
			{
				IInfoOverlayPanelModel.AddHudStringWithChildren(builder, target);
			}
		}
		else if (!role.IsVanillaRole())
		{
			var option = role.Loader.Get(RoleCommonOption.SpawnRate);
			IInfoOverlayPanelModel.AddHudStringWithChildren(builder, option);
		}

		string colorRoleName = role.GetColoredRoleName();
		string roleFullDesc = role.GetFullDescription();

		replaceAwakeRoleOptionString(ref roleOptionString, role);

		return (colorRoleName, roleFullDesc, roleOptionString);
	}

	private static (string, string, string) getMultiRoleInfoAndOption(MultiAssignRoleBase role)
	{
		string roleOptionString = "";

		if (!role.IsVanillaRole())
		{
			var useLoader =
				role.OffsetInfo is not null &&
				ExtremeRoleManager.CombRole.TryGetValue((byte)role.OffsetInfo.RoleId, out var combRole) &&
				combRole is not null ?
				combRole.Loader : role.Loader;


			var option = useLoader.Get(RoleCommonOption.SpawnRate);
			roleOptionString = IInfoOverlayPanelModel.ToHudStringWithChildren(option);
		}

		string colorRoleName = Design.ColoredString(
			role.GetNameColor(),
			Tr.GetString(role.RoleName));
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

	public void UpdateVisual()
	{

	}
}
