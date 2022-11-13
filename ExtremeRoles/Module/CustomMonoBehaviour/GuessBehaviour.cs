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

            public string GetRoleName()
            {
                if (this.AnothorId == ExtremeRoleId.Null)
                {
                    return Design.ColoedString(
                        getRoleColor(this.Id),
                        Translation.GetString(this.Id.ToString()));
                }
                else
                {
                    return string.Concat(
                        Design.ColoedString(
                            getRoleColor(this.Id),
                            Translation.GetString(this.Id.ToString())),
                        Design.ColoedString(
                            Palette.White,
                            " + "),
                        Design.ColoedString(
                            getRoleColor(this.AnothorId),
                            Translation.GetString(this.AnothorId.ToString())));
                }
            }
            private static Color getRoleColor(ExtremeRoleId target)
            {
                switch (target)
                {
                    case ExtremeRoleId.Sidekick:
                        return ColorPalette.JackalBlue;
                    case ExtremeRoleId.Servant:
                        return ColorPalette.QueenWhite;
                    case ExtremeRoleId.Doll:
                        return Palette.ImpostorRed;
                    default:
                        if (ExtremeRoleManager.NormalRole.TryGetValue(
                            (int)target, out SingleRoleBase role))
                        {
                            return role.GetNameColor(true);
                        }
                        else
                        {
                            foreach(var roleMng in ExtremeRoleManager.CombRole.Values)
                            {
                                foreach(var combRole in roleMng.Roles)
                                {
                                    if (combRole.Id == target)
                                    {
                                        return combRole.GetNameColor(true);
                                    }
                                }
                            }
                            return Palette.White;
                        }
                }
            }
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

        public string GetRoleName()
        {
            return this.info.GetRoleName();
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

