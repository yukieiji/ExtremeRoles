using System;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.InfoOverlay.FullDec
{
    internal class LocalPlayerGhostRoleShowTextBuilder : IShowTextBuilder
    {
        public LocalPlayerGhostRoleShowTextBuilder()
        { }

        public Tuple<string, string> GetShowText()
        {
            if (!CachedPlayerControl.LocalPlayer.Data.IsDead)
            {
                return Tuple.Create($"<size=200%>{Translation.GetString("yourAliveNow")}</size>\n", "");
            }
            var role = GhostRoles.ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (role == null)
            {
                return Tuple.Create($"<size=200%>{Translation.GetString("yourNoAssignGhostRole")}</size>\n", "");
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
                roleOptionString != "" ? $"{roleOptionString}" : "");

            return Tuple.Create(roleText, anotherRoleText);
        }
    }
}
