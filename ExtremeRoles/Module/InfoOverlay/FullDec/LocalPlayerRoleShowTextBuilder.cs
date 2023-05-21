using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Module.InfoOverlay.FullDec;

internal sealed class LocalPlayerRoleShowTextBuilder : IShowTextBuilder
{
    public LocalPlayerRoleShowTextBuilder()
    { }

    public (string, string, string) GetShowText()
    {
        string title = $"<size=200%>{Translation.GetString("yourRole")}</size>";
        string anotherRoleText = "";
        var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();
        var allOption = OptionManager.Instance;

        string roleOptionString = "";
        string colorRoleName;

        var multiAssignRole = role as MultiAssignRoleBase;
        bool isVanillaRole = role.IsVanillaRole();
        if (multiAssignRole != null)
        {
            if (!role.IsVanillaRole())
            {
                roleOptionString = allOption.GetHudStringWithChildren(
                    multiAssignRole.GetManagerOptionId(
                        RoleCommonOption.SpawnRate));
            }
            colorRoleName = Design.ColoedString(
                multiAssignRole.GetNameColor(),
                Translation.GetString(multiAssignRole.RoleName));
        }
        else
        {
            roleOptionString =
                allOption.GetHudStringWithChildren(role.GetRoleOptionId(
                    RoleCommonOption.SpawnRate));
            colorRoleName = role.GetColoredRoleName();
        }

        string roleFullDesc = role.GetFullDescription();
        replaceAwakeRoleOptionString(ref roleOptionString, role);

        string roleText = string.Concat(
            $"<size=150%>・{colorRoleName}</size>",
            roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
            $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
            roleOptionString != "" ? $"{roleOptionString}" : "");

        if (multiAssignRole != null)
        {
            var anotherRole = multiAssignRole.AnotherRole;

            if (anotherRole != null)
            {
                string anotherRoleOptionString = "";

                if (!anotherRole.IsVanillaRole())
                {
                    anotherRoleOptionString =
                        allOption.GetHudStringWithChildren(
                            multiAssignRole.AnotherRole.GetRoleOptionId(
                                RoleCommonOption.SpawnRate));
                }
                string anotherRoleFullDesc = anotherRole.GetFullDescription();
                replaceAwakeRoleOptionString(
                    ref anotherRoleOptionString, anotherRole);

                if (!isVanillaRole || anotherRoleOptionString != "")
                {
                    anotherRoleText =
                        $"<size=150%>・{multiAssignRole.AnotherRole.GetColoredRoleName()}</size>" +
                        (anotherRoleFullDesc != "" ? $"\n{anotherRoleFullDesc}\n" : "") +
                        $"・{Translation.GetString(multiAssignRole.AnotherRole.GetColoredRoleName())}{Translation.GetString("roleOption")}\n" +
                        (anotherRoleOptionString != "" ? $"{anotherRoleOptionString}" : "");
                }
            }
        }

        return (title, roleText, anotherRoleText);
    }

    private static void replaceAwakeRoleOptionString(
        ref string roleOptionString, SingleRoleBase role)
    {
        if (role is IRoleAwake<RoleTypes> awakeFromVaniraRole && 
            !awakeFromVaniraRole.IsAwake)
        {
            roleOptionString = "";
        }
        else if (
            role is IRoleAwake<Roles.ExtremeRoleId> awakeFromExRole && 
            !awakeFromExRole.IsAwake)
        {
            roleOptionString = awakeFromExRole.GetFakeOptionString();
        }
    }
}
