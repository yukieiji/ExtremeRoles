using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Carrier : SingleRoleBase, IRoleAbility
    {
        public DeadBody CarringBody;
        public float AlphaValue;
        public bool CanReportOnCarry;
        private GameData.PlayerInfo targetBody;

        public enum CarrierOption
        {
            CarryDistance,
            CanReportOnCarry,
        }

        private float carryDistance;

        public RoleAbilityButtonBase Button
        {
            get => this.carryButton;
            set
            {
                this.carryButton = value;
            }
        }

        private RoleAbilityButtonBase carryButton;

        public Carrier() : base(
            ExtremeRoleId.Carrier,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Carrier.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            CanReportOnCarry = false;
        }

        public static void CarryDeadBody(
            byte rolePlayerId, byte targetPlayerId)
        {
            var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
            var role = (Carrier)ExtremeRoleManager.GameRole[rolePlayerId];

            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetPlayerId)
                {
                    Color oldColor = array[i].bodyRenderer.color;

                    role.CarringBody = array[i];
                    role.CarringBody.transform.position = rolePlayer.transform.position;
                    role.CarringBody.transform.SetParent(rolePlayer.transform);
                    
                    role.AlphaValue = oldColor.a;
                    role.CarringBody.bodyRenderer.color = new Color(
                        oldColor.r, oldColor.g, oldColor.b, 0);
                    
                    if (!role.CanReportOnCarry)
                    {
                        role.CarringBody.GetComponentInChildren<BoxCollider2D>().enabled = false;
                    }
                    
                    break;
                }
            }
        }

        
        public static void PlaceDeadBody(
            byte rolePlayerId)
        {
            var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
            rolePlayer.StartCoroutine(
                deadBodyToReportablePosition(
                    rolePlayerId, rolePlayer).WrapToIl2Cpp());
        }

        private static IEnumerator deadBodyToReportablePosition(
            byte rolePlayerId,
            PlayerControl rolePlayer)
        {
            var role = (Carrier)ExtremeRoleManager.GameRole[rolePlayerId];

            role.CarringBody.transform.parent = null;


            if (!rolePlayer.inVent && !rolePlayer.moveable)
            {
                do
                {
                    yield return null;
                }
                while (!rolePlayer.moveable);
            }

            if (role.CarringBody == null) { yield break; }

            role.CarringBody.transform.position = rolePlayer.GetTruePosition() + new Vector2(0.15f, 0.15f);
            role.CarringBody.transform.position -= new Vector3(0.0f, 0.0f, 0.01f);


            Color color = role.CarringBody.bodyRenderer.color;
            role.CarringBody.bodyRenderer.color = new Color(
                color.r, color.g, color.b, role.AlphaValue);
            if (!role.CanReportOnCarry)
            {
                role.CarringBody.GetComponentInChildren<BoxCollider2D>().enabled = true;
            }
            role.CarringBody = null;
        }
        
        public void CreateAbility()
        {
            this.CreateReclickableAbilityButton(
                Translation.GetString("carry"),
                Loader.CreateSpriteFromResources(
                   Path.CarrierCarry),
                this.CleanUp);
        }

        public bool IsAbilityUse()
        {
            this.targetBody = Player.GetDeadBodyInfo(this.carryDistance);
            return this.IsCommonUse() && this.targetBody != null;
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
                RPCOperator.Command.CarrierCarryBody,
                new List<byte>
                { 
                    PlayerControl.LocalPlayer.PlayerId,
                    this.targetBody.PlayerId
                });

            CarryDeadBody(
                PlayerControl.LocalPlayer.PlayerId,
                this.targetBody.PlayerId);
            return true;
        }

        public void CleanUp()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.CarrierSetBody,
                new List<byte>{ PlayerControl.LocalPlayer.PlayerId });
            PlaceDeadBody(
                PlayerControl.LocalPlayer.PlayerId);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, 5.0f);

            CustomOption.Create(
                GetRoleOptionId((int)CarrierOption.CarryDistance),
                string.Concat(
                    this.RoleName,
                    CarrierOption.CarryDistance.ToString()),
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)CarrierOption.CanReportOnCarry),
                string.Concat(
                    this.RoleName,
                    CarrierOption.CanReportOnCarry.ToString()),
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.carryDistance = OptionHolder.AllOption[
                GetRoleOptionId((int)CarrierOption.CarryDistance)].GetValue();
            this.CanReportOnCarry = OptionHolder.AllOption[
                GetRoleOptionId((int)CarrierOption.CanReportOnCarry)].GetValue();
            this.RoleAbilityInit();
        }
    }
}
