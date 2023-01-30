using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

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

        public List<PlayerControl> NeedRoleAssignPlayer { get; private set; }

        private List<IPlayerToExRoleAssignData> assignData = new List<IPlayerToExRoleAssignData>();

        public PlayerRoleAssignData()
        {
            this.assignData.Clear();

            NeedRoleAssignPlayer = new List<PlayerControl>(
                PlayerControl.AllPlayerControls.ToArray());
        }

        public List<PlayerControl> GetCanImpostorAssignPlayer()
        {
            return NeedRoleAssignPlayer.FindAll(
                x =>
                {
                    return x.Data.Role.Role switch
                    {
                        RoleTypes.Impostor or RoleTypes.Shapeshifter => true,
                        _ => false
                    };
                });
        }
        public List<PlayerControl> GetCanCrewmateAssignPlayer()
        {
            return NeedRoleAssignPlayer.FindAll(
                x =>
                {
                    return x.Data.Role.Role switch
                    {
                        RoleTypes.Crewmate or 
                        RoleTypes.Engineer or
                        RoleTypes.Scientist => true,
                        
                        _ => false
                    };
                });
        }

        public void AddAssignData(IPlayerToExRoleAssignData data)
        {
            this.assignData.Add(data);
        }

        public void AddPlayer(PlayerControl player)
        {
            NeedRoleAssignPlayer.Add(player);
        }

        public void RemvePlayer(PlayerControl player)
        {
            NeedRoleAssignPlayer.RemoveAll(x => x.PlayerId == player.PlayerId);
        }

        public void Shuffle()
        {
            NeedRoleAssignPlayer = NeedRoleAssignPlayer.OrderBy(
                x => RandomGenerator.Instance.Next()).ToList();
        }
    }
}
