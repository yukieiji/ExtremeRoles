using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;

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
        private Dictionary<byte, ExtremeRoleType> combRoleAssignPlayerId = new Dictionary<byte, ExtremeRoleType>();

        public PlayerRoleAssignData()
        {
            this.assignData.Clear();

            this.NeedRoleAssignPlayer = new List<PlayerControl>(
                PlayerControl.AllPlayerControls.ToArray());
        }

        public void AllPlayerAssignToExRole()
        {
            using (var caller = RPCOperator.CreateCaller(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.SetRoleToAllPlayer))
            {
                caller.WritePackedInt(this.assignData.Count); // 何個あるか

                foreach (IPlayerToExRoleAssignData data in this.assignData)
                {
                    caller.WriteByte(data.PlayerId); // PlayerId
                    caller.WriteByte(data.RoleType); // RoleType : single or comb
                    caller.WritePackedInt(data.RoleId); // RoleId

                    if (data.RoleType == (byte)IPlayerToExRoleAssignData.ExRoleType.Comb)
                    {
                        var combData = (PlayerToCombRoleAssignData)data;
                        caller.WriteByte(combData.CombTypeId); // combTypeId
                        caller.WriteByte(combData.GameContId); // byted GameContId
                        caller.WriteByte(combData.AmongUsRoleId); // byted AmongUsVanillaRoleId
                    }
                }
            }
            RPCOperator.SetRoleToAllPlayer(this.assignData);
            RoleAssignState.Instance.SwitchRoleAssignToEnd();
            // instance = null;
        }

        public List<PlayerControl> GetCanImpostorAssignPlayer()
        {
            return this.NeedRoleAssignPlayer.FindAll(
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
            return this.NeedRoleAssignPlayer.FindAll(
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

        public bool TryGetCombRoleAssign(byte playerId, out ExtremeRoleType team)
        {
            return this.combRoleAssignPlayerId.TryGetValue(playerId, out team);
        }

        public void AddCombRoleAssignData(
            PlayerToCombRoleAssignData data, ExtremeRoleType team)
        {
            this.combRoleAssignPlayerId.Add(data.PlayerId, team);
            this.AddAssignData(data);
        }

        public void AddAssignData(IPlayerToExRoleAssignData data)
        {
            this.assignData.Add(data);
        }

        public void AddPlayer(PlayerControl player)
        {
            this.NeedRoleAssignPlayer.Add(player);
        }

        public void RemvePlayer(PlayerControl player)
        {
            this.NeedRoleAssignPlayer.RemoveAll(x => x.PlayerId == player.PlayerId);
        }

        public void Shuffle()
        {
            this.NeedRoleAssignPlayer = this.NeedRoleAssignPlayer.OrderBy(
                x => RandomGenerator.Instance.Next()).ToList();
        }
    }
}
