using System;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Module.InfoOverlay.FullDec
{
    internal class LocalPlayerRoleShowTextBuilder : IShowTextBuilder
    {
        public LocalPlayerRoleShowTextBuilder()
        { }

        public Tuple<string, string> GetShowText()
        {
            string roleText = $"<size=200%>{Translation.GetString("yourRole")}</size>\n";
            string anotherRoleText = "<size=200%> </size>\n";
            var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();
            var allOption = OptionHolder.AllOption;

            string roleOptionString = "";
            string colorRoleName;

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                roleOptionString =
                    CustomOption.AllOptionToString(
                        allOption[multiAssignRole.GetManagerOptionId(
                            RoleCommonOption.SpawnRate)]);
                colorRoleName = Design.ColoedString(
                    multiAssignRole.GetNameColor(),
                    Translation.GetString(multiAssignRole.RoleName));
            }
            else if (role.IsVanillaRole())
            {
                colorRoleName = role.GetColoredRoleName();
            }
            else
            {
                roleOptionString =
                    CustomOption.AllOptionToString(
                        allOption[role.GetRoleOptionId(
                            RoleCommonOption.SpawnRate)]);
                colorRoleName = role.GetColoredRoleName();
            }

            string roleFullDesc = role.GetFullDescription();
            var awakeFromVaniraRole = role as IRoleAwake<RoleTypes>;
            var awakeFromExRole = role as IRoleAwake<Roles.ExtremeRoleId>;
            if (awakeFromVaniraRole != null && !awakeFromVaniraRole.IsAwake)
            {
                roleOptionString = "";
            }
            else if (awakeFromExRole != null && !awakeFromExRole.IsAwake)
            {
                roleOptionString = awakeFromExRole.GetFakeOptionString();
            }

            roleText += string.Concat(
                $"<size=150%>・{colorRoleName}</size>",
                roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                roleOptionString != "" ? $"{roleOptionString}" : "");

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {

                    string anotherRoleOptionString = "";

                    if (!multiAssignRole.AnotherRole.IsVanillaRole())
                    {
                        anotherRoleOptionString =
                            CustomOption.AllOptionToString(
                                allOption[multiAssignRole.AnotherRole.GetRoleOptionId(
                                    RoleCommonOption.SpawnRate)]);
                    }
                    string anotherRoleFullDesc = multiAssignRole.AnotherRole.GetFullDescription();

                    awakeFromVaniraRole = multiAssignRole.AnotherRole as IRoleAwake<RoleTypes>;
                    awakeFromExRole = multiAssignRole.AnotherRole as IRoleAwake<Roles.ExtremeRoleId>;
                    if (awakeFromVaniraRole != null && !awakeFromVaniraRole.IsAwake)
                    {
                        anotherRoleOptionString = "";
                    }
                    else if (awakeFromVaniraRole != null && !awakeFromExRole.IsAwake)
                    {
                        anotherRoleOptionString = awakeFromExRole.GetFakeOptionString();
                    }

                    anotherRoleText +=
                        $"\n<size=150%>・{multiAssignRole.AnotherRole.GetColoredRoleName()}</size>" +
                        (anotherRoleFullDesc != "" ? $"\n{anotherRoleFullDesc}\n" : "") +
                        $"・{Translation.GetString(multiAssignRole.AnotherRole.GetColoredRoleName())}{Translation.GetString("roleOption")}\n" +
                        (anotherRoleOptionString != "" ? $"{anotherRoleOptionString}" : "");
                }
            }

            return Tuple.Create(roleText, anotherRoleText);
        }
    }
}
