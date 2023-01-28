using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.RoleAssign
{
    internal sealed class RoleAssignPlayerList
    {
        internal Dictionary<ExtremeRoleType, List<PlayerControl>> RoleAssignPlayer { get; private set; }

        internal RoleAssignPlayerList(bool useXion, int neutralPlayerNum)
        {

            // やりたいこと
            // 1. シオンのプレイヤーを除外シマース
            // 2. インポスターのプレイヤーとクルーのプレイヤーを分割シマース
            // 3. クルーのプレイヤーをシャッフルシマース
            // 4. 最初からニュートラルのプレイヤーを規定数取り出シマース
            // 5. クルーとニュートラルのプレイヤーをシャッフルシマース

            List<PlayerControl> allPlayer = new List<PlayerControl>(PlayerControl.AllPlayerControls.ToArray());
            
            // ホストプレイヤー除外
            if (useXion && ExtremeGameModeManager.Instance.RoleSelector.CanUseXion)
            {
                allPlayer.RemoveAll(x => x.PlayerId == PlayerControl.LocalPlayer.PlayerId);
            }

            Dictionary<ExtremeRoleType, List<PlayerControl>> separatedPlayer = allPlayer.GroupBy(
                x => x.Data.Role.Role switch
                {
                    RoleTypes.Impostor or RoleTypes.Shapeshifter
                        => ExtremeRoleType.Impostor,

                    RoleTypes.Crewmate or RoleTypes.Engineer or RoleTypes.Engineer
                        => ExtremeRoleType.Crewmate,

                    _ => ExtremeRoleType.Null,
                }).ToDictionary(g => g.Key, g => g.ToList());

            List<PlayerControl> crewAssignRolePlayer = separatedPlayer[ExtremeRoleType.Crewmate];

            List<PlayerControl> neutralAssignPlayer = crewAssignRolePlayer.OrderBy(
                x => RandomGenerator.Instance.Next()).Take(
                    Math.Clamp(neutralPlayerNum, 0, crewAssignRolePlayer.Count)).ToList();

            RoleAssignPlayer = new Dictionary<ExtremeRoleType, List<PlayerControl>>
            {
                { 
                    ExtremeRoleType.Crewmate, 
                    separatedPlayer[ExtremeRoleType.Crewmate].Where(
                        x => !neutralAssignPlayer.Contains(x)).OrderBy(
                            x => RandomGenerator.Instance.Next()).ToList()
                },
                {
                    ExtremeRoleType.Impostor,
                    separatedPlayer[ExtremeRoleType.Impostor].OrderBy(
                        x => RandomGenerator.Instance.Next()).ToList()
                },
                {
                    ExtremeRoleType.Neutral,
                    neutralAssignPlayer.OrderBy(
                        x => RandomGenerator.Instance.Next()).ToList()
                },
            };
        }
        internal void RemovePlayer(PlayerControl removePlayer)
        {
            foreach (var assignPlayer in this.RoleAssignPlayer.Values)
            {
                assignPlayer.RemoveAll(x => x.PlayerId == removePlayer.PlayerId);
            }
        }
    }
}
