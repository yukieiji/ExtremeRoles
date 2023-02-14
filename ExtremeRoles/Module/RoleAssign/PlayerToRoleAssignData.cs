using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign
{
    public struct PlayerToSingleRoleAssignData : IPlayerToExRoleAssignData
    {
        public byte PlayerId
        {
            get => playerId;
        }
        public int RoleId
        {
            get => roleId;
        }
        public int ControlId
        {
            get => controlId;
        }

        public byte RoleType { get => (byte)IPlayerToExRoleAssignData.ExRoleType.Single; }

        private byte playerId;
        private int roleId;
        private int controlId;

        public PlayerToSingleRoleAssignData(
            byte playerId, int roleId, int controlId)
        {
            this.playerId = playerId;
            this.roleId = roleId;
            this.controlId = controlId;
        }
    }

    public struct PlayerToCombRoleAssignData : IPlayerToExRoleAssignData
    {
        public byte PlayerId
        {
            get => playerId;
        }
        public int RoleId
        {
            get => roleId;
        }
        public int ControlId
        {
            get => gameContId;
        }

        public byte RoleType { get => (byte)IPlayerToExRoleAssignData.ExRoleType.Comb; }

        public byte CombTypeId => combTypeId;
        public byte AmongUsRoleId => amongUsRoleId;

        private int roleId;
        private int gameContId;

        private byte playerId;
        private byte combTypeId;
        private byte amongUsRoleId;

        public PlayerToCombRoleAssignData(
            byte playerId, int roleId,
            byte combType, int gameContId,
            byte amongUsRoleId)
        {
            this.playerId = playerId;
            this.roleId = roleId;
            combTypeId = combType;
            this.gameContId = gameContId;
            this.amongUsRoleId = amongUsRoleId;
        }
    }
}
