using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.Module
{
    public interface IAssignedPlayer
    {
        enum ExRoleType : byte
        {
            Single,
            Comb,
        }

        public byte PlayerId { get; }
        public byte RoleId { get; }
        public byte RoleType { get; }
    }

    public struct AssignedPlayerToSingleRoleData : IAssignedPlayer
    {
        public byte PlayerId
        {
            get => playerId;
        }
        public byte RoleId
        {
            get => roleId;
        }
        public byte RoleType { get => (byte)IAssignedPlayer.ExRoleType.Single; }

        private byte playerId;
        private byte roleId;

        public AssignedPlayerToSingleRoleData(
            byte playerId, byte roleId)
        {
            this.playerId = playerId;
            this.roleId = roleId;
        }

    }

    public struct AssignedPlayerToCombRoleData : IAssignedPlayer
    {
        public byte PlayerId
        {
            get => playerId;
        }
        public byte RoleId
        {
            get => roleId;
        }
        public byte RoleType { get => (byte)IAssignedPlayer.ExRoleType.Comb; }

        public byte CombTypeId => combTypeId;

        public byte GameContId => this.gameContId;
        public byte AmongUsRoleId => this.amongUsRoleId;

        private byte playerId;
        private byte roleId;
        private byte combTypeId;
        private byte gameContId;
        private byte amongUsRoleId;

        public AssignedPlayerToCombRoleData(
            byte playerId, byte roleId,
            byte combType, byte gameContId,
            byte amongUsRoleId)
        {
            this.playerId = playerId;
            this.roleId = roleId;
            this.combTypeId = combType;
            this.gameContId = gameContId;
            this.amongUsRoleId = amongUsRoleId;
        }
    }
}
