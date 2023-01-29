using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign
{
    public sealed class PlayerRoleAssignData
    {
        public static PlayerRoleAssignData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerRoleAssignData();
                }
                return instance;
            }
        }
        private static PlayerRoleAssignData instance = null;

        public List<PlayerControl> AllNotAssignPlayer { get; private set; }

        private List<IPlayerToExRoleAssignData> assignData = new List<IPlayerToExRoleAssignData>();

        public PlayerRoleAssignData()
        {
            this.assignData.Clear();

            AllNotAssignPlayer = new List<PlayerControl>(
                PlayerControl.AllPlayerControls.ToArray());
        }

        public void AddAssignData(IPlayerToExRoleAssignData data)
        {
            this.assignData.Add(data);
        }

        public void AddPlayer(PlayerControl player)
        {
            AllNotAssignPlayer.Add(player);
        }

        public void RemvePlayer(PlayerControl player)
        {
            AllNotAssignPlayer.RemoveAll(x => x.PlayerId == player.PlayerId);
        }

        public void Shuffle()
        {
            AllNotAssignPlayer = AllNotAssignPlayer.OrderBy(
                x => RandomGenerator.Instance.Next()).ToList();
        }
    }
}
