using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.GhostRoles;

namespace ExtremeRoles.Module.InfoOverlay.FullDec
{
    internal sealed class LocalPlayerGhostRoleShowTextBuilder : IShowTextBuilder
    {
        public LocalPlayerGhostRoleShowTextBuilder()
        { }

        public (string, string, string) GetShowText()
        {
            if (!CachedPlayerControl.LocalPlayer.Data.IsDead)
            {
                return ($"<size=200%>{Translation.GetString("yourAliveNow")}</size>\n", "", "");
            }
            var role = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (role == null)
            {
                return ($"<size=200%>{Translation.GetString("yourNoAssignGhostRole")}</size>\n", "", "");
            }
            string title = $"<size=200%>{Translation.GetString("yourGhostRole")}</size>";
            string anotherRoleText = "<size=200%> </size>\n";
            var allOption = OptionHolder.AllOption;

            string roleOptionString = "";
            string colorRoleName = role.GetColoredRoleName();

            if (!role.IsVanillaRole())
            {
                if (!allOption.TryGetValue(
                        role.GetRoleOptionId(RoleCommonOption.SpawnRate), out IOption spawnOption))
                {
                    var aliveRole = (MultiAssignRoleBase)ExtremeRoleManager.GetLocalPlayerRole();
                    spawnOption = allOption[aliveRole.GetManagerOptionId(RoleCommonOption.SpawnRate)];
                }
                roleOptionString = spawnOption.ToHudStringWithChildren();
            }

            string roleFullDesc = role.GetFullDescription();

            string roleText = string.Concat(
                $"<size=150%>・{colorRoleName}</size>",
                roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                roleOptionString != "" ? $"{roleOptionString}" : "");

            return (title, roleText, anotherRoleText);
        }
    }
}
