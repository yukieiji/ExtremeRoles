using System;
using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.InfoOverlay
{
    internal class GhostRolesFullDecManager : IFullDecManager
    {
        private int rolePage = 0;
        private List<(string, int)> allGhostRoleText = new List<(string, int)>();
        
        public GhostRolesFullDecManager()
        {
            this.Clear();
        }
        
        public void Clear()
        {
            allGhostRoleText.Clear();
            rolePage = 0;
        }

        public void ChangeRoleInfoPage(int count)
        {
            if (this.allGhostRoleText.Count == 0) { return; }
            this.rolePage = (this.rolePage + count) % this.allGhostRoleText.Count;

            if (this.rolePage < 0)
            {
                this.rolePage = this.allGhostRoleText.Count + this.rolePage;
            }
        }

        public Tuple<string, string> GetPlayerRoleText()
        {
            var role = GhostRoles.ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (role == null)
            {
                return Tuple.Create($"<size=200%>{Translation.GetString("yourAliveNow")}</size>\n", "");
            }
            string roleText = $"<size=200%>{Translation.GetString("yourGhostRole")}</size>\n";
            string anotherRoleText = "<size=200%> </size>\n";
            var allOption = OptionHolder.AllOption;

            string roleOptionString = "";
            string colorRoleName;

            if (role.IsVanillaRole())
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

            roleText += string.Concat(
                $"<size=150%>・{colorRoleName}</size>",
                roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                (roleOptionString != "") ? $"{roleOptionString}" : "");

            return Tuple.Create(roleText, anotherRoleText);
        }

        public Tuple<string, string> GetRoleInfoPageText()
        {
            if (this.allGhostRoleText.Count == 0) { createAllRoleText(); }

            var (roleTextBase, optionId) = this.allGhostRoleText[this.rolePage];

            string roleOption = CustomOption.AllOptionToString(
                OptionHolder.AllOption[optionId + (int)RoleCommonOption.SpawnRate]);

            string showRole = string.Concat(
                $"<size=200%>{Translation.GetString("ghostRoleDesc")}</size>",
                $"           {Translation.GetString("changeRoleMore")}",
                $"({this.rolePage + 1}/{this.allGhostRoleText.Count})\n",
                string.Format(roleTextBase, roleOption != "" ? $"{roleOption}" : ""));
            return Tuple.Create(showRole, "");
        }
        private void createAllRoleText()
        {
            int optionId;
            string colorRoleName;
            string roleFullDesc;
            string roleText;

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

                    roleText = string.Concat(
                        $"<size=150%>・{colorRoleName}</size>",
                        roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                        $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                        "{0}");

                    this.allGhostRoleText.Add(((string)roleText.Clone(), optionId));
                }
            }


            foreach (var role in GhostRoles.ExtremeGhostRoleManager.AllGhostRole.Values)
            {
                optionId = role.OptionOffset;
                colorRoleName = role.GetColoredRoleName();

                roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
                roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

                roleText = string.Concat(
                    $"<size=150%>・{colorRoleName}</size>",
                    roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                    $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                    "{0}");

                this.allGhostRoleText.Add(((string)roleText.Clone(), optionId));
            }
        }
    }
}
