using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.RoleSelector
{
    public abstract class RoleSelectorBase
    {
        public bool IsRoleSetUpEnd { get; private set; }

        public abstract bool IsXionUse { get; }

        private List<IAssignedPlayer> assignData = new List<IAssignedPlayer>();
        private HashSet<byte> readyPlayer = new HashSet<byte>();

        public bool IsAssignReady() => readyPlayer.Count ==
            (CachedPlayerControl.AllPlayerControls.Count - 1);

        public void AddReadyPlayer(byte playerId)
        {
            if (!AmongUsClient.Instance.AmHost) { return; }

            Logging.Debug($"ReadyPlayer:{playerId}");

            this.readyPlayer.Add(playerId);
        }

        public void RpcAssignAllRole()
        {
            using (var caller = RPCOperator.CreateCaller(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.SetRoleToAllPlayer))
            {
                caller.WritePackedInt(this.assignData.Count); // 何個あるか

                foreach (IAssignedPlayer data in this.assignData)
                {
                    caller.WriteByte(data.PlayerId); // PlayerId
                    caller.WriteByte(data.RoleType); // RoleType : single or comb
                    caller.WritePackedInt(data.RoleId); // RoleId

                    if (data.RoleType == (byte)IAssignedPlayer.ExRoleType.Comb)
                    {
                        var combData = (AssignedPlayerToCombRoleData)data;
                        caller.WriteByte(combData.CombTypeId); // combTypeId
                        caller.WriteByte(combData.GameContId); // byted GameContId
                        caller.WriteByte(combData.AmongUsRoleId); // byted AmongUsVanillaRoleId
                    }
                }
            }
            RPCOperator.SetRoleToAllPlayer(this.assignData);

            this.assignData.Clear();
            this.readyPlayer.Clear();
        }

        public void RpcLocalPlayerToReady()
        {
            using (var caller = RPCOperator.CreateCaller(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.SetUpReady))
            {
                caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
            }
        }

        public void SwitchToRoleAssingnEnd()
        {
            IsRoleSetUpEnd = true;
        }

        public void CreateRoleAssignData()
        {
            this.assignData = GetRoleAssignData();
        }

        public virtual void SetUp()
        {
            this.readyPlayer.Clear();
            this.assignData.Clear();

            this.IsRoleSetUpEnd = false;
        }

        public void SetUpXion()
        {
            if (!IsXionUse) { return; }

            PlayerControl loaclPlayer = PlayerControl.LocalPlayer;
            this.assignData.Add(new AssignedPlayerToSingleRoleData(
                loaclPlayer.PlayerId, (int)ExtremeRoleId.Xion));
            loaclPlayer.RpcSetRole(AmongUs.GameOptions.RoleTypes.Crewmate);
            loaclPlayer.Data.IsDead = true;
        }

        public abstract bool IsAssignGhostRole();
        public abstract bool RpcTryAssignGhostRole();
        public abstract SingleRoleBase GetSingleRole(ExtremeRoleId id);
        public abstract CombinationRoleManagerBase GetCombinationRoleManager(CombinationRoleType id);
        public abstract List<string> GetRoleSelectorOptionList();
        public abstract void CreateRoleSelectorOptionMenu();

        protected abstract List<IAssignedPlayer> GetRoleAssignData();
    }
}
