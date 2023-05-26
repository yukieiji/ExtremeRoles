using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.InfoOverlay.FullDec;

internal sealed class AllRoleShowTextBuilder : PageShowTextBuilderBase
{
    public AllRoleShowTextBuilder() : base()
    { }

    public override (string, string, string) GetShowText()
    {
        if (this.AllPage.Count == 0) { createAllRoleText(); }

        var (roleTextBase, optionId) = this.AllPage[this.Page];

        string roleOption = OptionManager.Instance.GetHudStringWithChildren(
            optionId + (int)RoleCommonOption.SpawnRate);

        string title = string.Concat(
            $"<size=200%>{Translation.GetString("roleDesc")}</size>",
            $"           {Translation.GetString("changeRoleMore")}",
            $"({this.Page + 1}/{this.AllPage.Count})");


        return (title, string.Format(roleTextBase, roleOption != "" ? $"{roleOption}" : ""), "");
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

            this.AllPage.Add(((string)roleText.Clone(), optionId));
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

                        this.AllPage.Add(((string)roleText.Clone(), optionId));
                    }
                }
                else if (combRole is FlexibleCombinationRoleManagerBase flexCombRole)
                {
                    optionId = flexCombRole.GetOptionIdOffset();
                    colorRoleName = flexCombRole.GetOptionName();

                roleFullDesc = flexCombRole.GetBaseRoleFullDescription();
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
