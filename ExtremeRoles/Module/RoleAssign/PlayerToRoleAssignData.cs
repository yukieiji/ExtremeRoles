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
        public byte RoleType { get => (byte)IPlayerToExRoleAssignData.ExRoleType.Single; }

        private byte playerId;
        private int roleId;

        public PlayerToSingleRoleAssignData(
            byte playerId, int roleId)
        {
            this.playerId = playerId;
            this.roleId = roleId;
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
        public byte RoleType { get => (byte)IPlayerToExRoleAssignData.ExRoleType.Comb; }

        public byte CombTypeId => combTypeId;

        public byte GameContId => gameContId;
        public byte AmongUsRoleId => amongUsRoleId;

        private byte playerId;
        private int roleId;
        private byte combTypeId;
        private byte gameContId;
        private byte amongUsRoleId;

        public PlayerToCombRoleAssignData(
            byte playerId, int roleId,
            byte combType, byte gameContId,
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
