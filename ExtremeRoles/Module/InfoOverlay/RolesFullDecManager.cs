using System;
using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Module.InfoOverlay
{
    internal class RolesFullDecManager : IFullDecManager
    {
        private int rolePage = 0;
        private List<(string, int)> allRoleText = new List<(string, int)>();
        
        public RolesFullDecManager()
        {
            this.Clear();
        }
        
        public void Clear()
        {
            allRoleText.Clear();
            rolePage = 0;
        }

        public void ChangeRoleInfoPage(int count)
        {
            if (this.allRoleText.Count == 0) { return; }
            this.rolePage = (this.rolePage + count) % this.allRoleText.Count;

            if (this.rolePage < 0)
            {
                this.rolePage = this.allRoleText.Count + this.rolePage;
            }
        }

        public Tuple<string, string> GetPlayerRoleText()
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
                (roleOptionString != "") ? $"{roleOptionString}" : "");

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

        public Tuple<string, string> GetRoleInfoPageText()
        {
            if (this.allRoleText.Count == 0) { createAllRoleText(); }

            var (roleTextBase, optionId) = this.allRoleText[this.rolePage];

            string roleOption = CustomOption.AllOptionToString(
                OptionHolder.AllOption[optionId + (int)RoleCommonOption.SpawnRate]);

            string showRole = string.Concat(
                $"<size=200%>{Translation.GetString("roleDesc")}</size>",
                $"           {Translation.GetString("changeRoleMore")}",
                $"({this.rolePage + 1}/{this.allRoleText.Count})\n",
                string.Format(roleTextBase, roleOption != "" ? $"{roleOption}" : ""));
            return Tuple.Create(showRole, "");
        }
        private void createAllRoleText()
        {
            int optionId;
            string colorRoleName;
            string roleFullDesc;
            string roleText;

            foreach (var role in Roles.ExtremeRoleManager.NormalRole.Values)
            {
                optionId = role.GetRoleOptionOffset();
                colorRoleName = role.GetColoredRoleName(true);

                roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
                roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

                roleText = string.Concat(
                    $"<size=150%>・{colorRoleName}</size>",
                    roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                    $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                    "{0}");

                this.allRoleText.Add(((string)roleText.Clone(), optionId));
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

                        roleText = string.Concat(
                            $"<size=150%>・{colorRoleName}</size>",
                            roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                            $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                            "{0}");

                        this.allRoleText.Add(((string)roleText.Clone(), optionId));
                    }
                }
                else if (combRole is FlexibleCombinationRoleManagerBase)
                {

                    var role = ((FlexibleCombinationRoleManagerBase)combRole).BaseRole;

                    optionId = role.GetManagerOptionOffset();
                    colorRoleName = role.GetColoredRoleName();

                    roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
                    roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

                    roleText = string.Concat(
                        $"<size=150%>・{colorRoleName}</size>",
                        roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                        $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                        "{0}");

                    this.allRoleText.Add(((string)roleText.Clone(), optionId));
                }
            }
        }
    }
}
