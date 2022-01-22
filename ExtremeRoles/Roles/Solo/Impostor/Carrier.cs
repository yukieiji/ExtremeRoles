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
    public class Carrier : SingleRoleBase, IRoleAbility
    {
        public DeadBody CarringBody;
        public float alphaValue;
        public Transform Parent;
        private GameData.PlayerInfo targetBody;

        public enum CarrierOption
        {
            CarryDistance,
            CanReportOnCarry,
        }

        private float carryDistance;
        private bool canReportOnCarry;

        public Carrier() : base(
            ExtremeRoleId.Carrier,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Carrier.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            canReportOnCarry = false;
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.carryButton;
            set
            {
                this.carryButton = value;
            }
        }

        private RoleAbilityButtonBase carryButton;

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
                    role.Parent = array[i].transform.parent;
                    Color oldColor = array[i].bodyRenderer.color;

                    role.CarringBody = array[i];
                    role.CarringBody.transform.position = rolePlayer.transform.position;
                    role.CarringBody.transform.SetParent(rolePlayer.transform);
                    
                    role.alphaValue = oldColor.a;
                    role.CarringBody.bodyRenderer.color = new Color(
                        oldColor.r, oldColor.g, oldColor.b, 0);
                    
                    if (!role.canReportOnCarry)
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
            var role = (Carrier)ExtremeRoleManager.GameRole[rolePlayerId];
            var rolePlayer = Player.GetPlayerControlById(rolePlayerId);

            role.CarringBody.transform.parent = null;
            role.CarringBody.transform.position = rolePlayer.transform.position;
            
            
            Color color = role.CarringBody.bodyRenderer.color;
            role.CarringBody.bodyRenderer.color = new Color(
                color.r, color.g, color.b, role.alphaValue);
            if (!role.canReportOnCarry)
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
                   Path.CarrierCarry, 115f),
                this.CleanUp);
        }

        public bool IsAbilityUse()
        {
            setTargetDeadBody();
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
                parentOps, true);

            CustomOption.Create(
                GetRoleOptionId((int)CarrierOption.CarryDistance),
                string.Concat(
                    this.RoleName,
                    CarrierOption.CanReportOnCarry.ToString()),
                1.0f, 1.0f, 5.0f, 1.0f,
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
            this.canReportOnCarry = OptionHolder.AllOption[
                GetRoleOptionId((int)CarrierOption.CanReportOnCarry)].GetValue();
            this.RoleAbilityInit();
        }

        private void setTargetDeadBody()
        {
            this.targetBody = null;

            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(
                PlayerControl.LocalPlayer.GetTruePosition(),
                this.carryDistance,
                Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();

                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        if ((Vector2.Distance(truePosition2, truePosition) <= this.carryDistance) &&
                            (PlayerControl.LocalPlayer.CanMove) &&
                            (!PhysicsHelpers.AnythingBetween(
                                truePosition, truePosition2, Constants.ShipAndObjectsMask, false)))
                        {
                            this.targetBody = GameData.Instance.GetPlayerById(component.ParentId);
                            break;
                        }
                    }
                }
            }
        }
    }
}
