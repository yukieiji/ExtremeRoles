using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Cracker : SingleRoleBase, IRoleAbility
    {
        private byte targetDeadBodyId;

        public enum CrackerOption
        {
            RemoveDeadBody,
            CanCrackDistance,
        }

        public bool IsRemoveDeadBody;
        private float crackDistance;

        public Cracker() : base(
            ExtremeRoleId.Cracker,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Cracker.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public RoleAbilityButtonBase Button
        { 
            get => this.crackButton;
            set
            {
                this.crackButton = value;
            }
        }

        private RoleAbilityButtonBase crackButton;

        public static void CrackDeadBody(
            byte rolePlayerId, byte targetPlayerId)
        {
            var role = (Cracker)ExtremeRoleManager.GameRole[rolePlayerId];

            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
                {

                    if (role.IsRemoveDeadBody)
                    {
                        Object.Destroy(array[i].gameObject);
                    }
                    else
                    {
                        array[i].GetComponentInChildren<BoxCollider2D>().enabled = false; ;
                    }
                    break;
                }
            }
        }
        
        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("paint"),
                Loader.CreateSpriteFromResources(
                   Path.PainterPaint));
        }

        public bool IsAbilityUse()
        {
            this.targetDeadBodyId = byte.MaxValue;
            GameData.PlayerInfo info = Player.GetDeadBodyInfo(
                this.crackDistance);
            
            if (info != null)
            {
                this.targetDeadBodyId = info.PlayerId;
            }

            return this.IsCommonUse() && this.targetDeadBodyId != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.PainterPaintBody,
                new List<byte>
                { 
                    PlayerControl.LocalPlayer.PlayerId,
                    this.targetDeadBodyId
                });

            CrackDeadBody(
                PlayerControl.LocalPlayer.PlayerId,
                this.targetDeadBodyId);
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)CrackerOption.CanCrackDistance),
                string.Concat(
                    this.RoleName,
                    CrackerOption.CanCrackDistance.ToString()),
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)CrackerOption.RemoveDeadBody),
                string.Concat(
                    this.RoleName,
                    CrackerOption.RemoveDeadBody.ToString()),
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.crackDistance = OptionHolder.AllOption[
                GetRoleOptionId((int)CrackerOption.CanCrackDistance)].GetValue();
            this.IsRemoveDeadBody = OptionHolder.AllOption[
                GetRoleOptionId((int)CrackerOption.RemoveDeadBody)].GetValue();
            this.RoleAbilityInit();
        }
    }
}
