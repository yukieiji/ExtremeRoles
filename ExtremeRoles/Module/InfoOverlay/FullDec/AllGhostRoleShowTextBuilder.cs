using System;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.InfoOverlay.FullDec
{
    internal class AllGhostRoleShowTextBuilder : PageShowTextBuilderBase
    {
        public AllGhostRoleShowTextBuilder() : base()
        { }

        public override Tuple<string, string> GetShowText()
        {
            if (this.AllPage.Count == 0) { createAllRoleText(); }

            var (roleTextBase, optionId) = this.AllPage[this.Page];

            string roleOption = CustomOption.AllOptionToString(
                OptionHolder.AllOption[optionId + (int)RoleCommonOption.SpawnRate]);

            string showRole = string.Concat(
                $"<size=200%>{Translation.GetString("ghostRoleDesc")}</size>",
                $"           {Translation.GetString("changeGhostRoleMore")}",
                $"({this.Page + 1}/{this.AllPage.Count})\n",
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

                    this.AllPage.Add(((string)roleText.Clone(), optionId));
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

                this.AllPage.Add(((string)roleText.Clone(), optionId));
            }
        }

    }
}
