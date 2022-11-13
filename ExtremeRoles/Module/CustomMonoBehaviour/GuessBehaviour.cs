using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class GuessBehaviour : MonoBehaviour
    {
        public struct RoleInfo
        {
            public ExtremeRoleId Id;
            public ExtremeRoleId AnothorId;
            public ExtremeRoleType Team;
        }

        private byte playerId;
        private string playerName;

        private RoleInfo info;
        private Action<RoleInfo, byte> guessAction;

        public GuessBehaviour(IntPtr ptr) : base(ptr) { }

        public Action GetGuessAction()
        {
            return () =>
            {
                this.guessAction.Invoke(this.info, this.playerId);
            };
        }

        public void Create(RoleInfo info, Action<RoleInfo, byte> guessAction)
        {
            this.info = info;
            this.guessAction = guessAction;
        }

        public string GetButtonText()
        {
            return string.Empty;
        }

        public string GetConfirmText()
        {
            return string.Format(
                Translation.GetString("guessCheck"),
                this.playerName);
        }

        public void SetTarget(byte playerId)
        {
            this.playerId = playerId;
            this.playerName = GameData.Instance.GetPlayerById(playerId)?.DefaultOutfit.PlayerName;
        }
    }
}

