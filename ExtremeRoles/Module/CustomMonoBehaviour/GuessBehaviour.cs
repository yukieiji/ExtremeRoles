using System;
using System.Collections.Generic;

using UnityEngine;
using UnhollowerBaseLib.Attributes;

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

            private static HashSet<RoleTypes> vanilaCrew = new HashSet<RoleTypes>()
            {
                RoleTypes.Crewmate,
                RoleTypes.Scientist,
                RoleTypes.Engineer,
            };

            public string GetRoleName()
            {
                string basicRoleName = convertIdToRoleName(this.Id);

                if (this.AnothorId == ExtremeRoleId.Null)
                {
                    return basicRoleName;
                }
                else
                {
                    return string.Concat(
                        basicRoleName,
                        Design.ColoedString(
                            Palette.White,
                            " + "),
                        convertIdToRoleName(this.AnothorId));
                }
            }
            public override string ToString()
            {
                return $"Team:{this.Team}  Role:{this.Id}＆{this.AnothorId}";
            }

            private static string convertIdToRoleName(ExtremeRoleId id)
            {
                RoleTypes castedId = (RoleTypes)id;
                bool isVanila = Enum.IsDefined(typeof(RoleTypes), castedId);
                string roleName = isVanila ?
                    Design.ColoedString(
                        vanilaCrew.Contains(castedId) ?
                        Palette.White : Palette.ImpostorRed,
                        Translation.GetString(castedId.ToString())) :
                    Design.ColoedString(
                        getRoleColor(id),
                        Translation.GetString(id.ToString()));
                return roleName;
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

        [HideFromIl2Cpp]
        public Action GetGuessAction()
        {
            return () =>
            {
                this.guessAction.Invoke(this.info, this.playerId);
            };
        }

        [HideFromIl2Cpp]
        public void Create(RoleInfo info, Action<RoleInfo, byte> guessAction)
        {
            this.info = info;
            this.guessAction = guessAction;
        }

        public string GetRoleName()
        {
            return this.info.GetRoleName();
        }

        public string GetPlayerName() => this.playerName;

        public void SetTarget(byte playerId)
        {
            this.playerId = playerId;
            this.playerName = GameData.Instance.GetPlayerById(playerId)?.DefaultOutfit.PlayerName;
        }
    }
}

