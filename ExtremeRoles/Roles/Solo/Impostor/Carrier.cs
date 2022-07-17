using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Carrier : SingleRoleBase, IRoleAbility, IRoleSpecialReset
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

        public static void Ability(
            byte rolePlayerId, float x, float y,
            byte targetPlayerId, bool isCarry)
        {
            var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
            var role = ExtremeRoleManager.GetSafeCastedRole<Carrier>(rolePlayerId);
            if (role == null || rolePlayer == null) { return; }

            rolePlayer.NetTransform.SnapTo(new Vector2(x, y));

            if (isCarry)
            {
                carryDeadBody(role, rolePlayer, targetPlayerId);
            }
            else
            {
                rolePlayer.StartCoroutine(
                    deadBodyToReportablePosition(
                        role, rolePlayer).WrapToIl2Cpp());
            }
        }

        private static void carryDeadBody(
            Carrier role, PlayerControl rolePlayer, byte targetPlayerId)
        {
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

        private static IEnumerator deadBodyToReportablePosition(
            Carrier role,
            PlayerControl rolePlayer)
        {
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

            Vector2 pos = rolePlayer.GetTruePosition();
            role.CarringBody.transform.position = new Vector3(pos.x, pos.y, (pos.y / 1000f));

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
            PlayerControl player = CachedPlayerControl.LocalPlayer;
            Vector3 pos = player.transform.position;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                player.NetId, (byte)RPCOperator.Command.CarrierAbility,
                Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(this.targetBody.PlayerId);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            carryDeadBody(
                this, CachedPlayerControl.LocalPlayer,
                this.targetBody.PlayerId);
            return true;
        }

        public void CleanUp()
        {
            PlayerControl player = CachedPlayerControl.LocalPlayer;
            Vector3 pos = player.transform.position;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                player.NetId, (byte)RPCOperator.Command.CarrierAbility,
                Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(byte.MinValue);
            writer.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            player.StartCoroutine(
                deadBodyToReportablePosition(
                    this, player).WrapToIl2Cpp());
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, 5.0f);

            CreateFloatOption(
                CarrierOption.CarryDistance,
                1.0f, 1.0f, 5.0f, 0.5f,
                parentOps);

            CreateBoolOption(
                CarrierOption.CanReportOnCarry,
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.carryDistance = OptionHolder.AllOption[
                GetRoleOptionId(CarrierOption.CarryDistance)].GetValue();
            this.CanReportOnCarry = OptionHolder.AllOption[
                GetRoleOptionId(CarrierOption.CanReportOnCarry)].GetValue();
            this.RoleAbilityInit();
        }

        public void AllReset(PlayerControl rolePlayer)
        {
            if (this.CarringBody != null)
            {
                this.CarringBody.transform.parent = null;
                this.CarringBody.transform.position = rolePlayer.GetTruePosition() + new Vector2(0.15f, 0.15f);
                this.CarringBody.transform.position -= new Vector3(0.0f, 0.0f, 0.01f);


                Color color = this.CarringBody.bodyRenderer.color;
                this.CarringBody.bodyRenderer.color = new Color(
                    color.r, color.g, color.b, this.AlphaValue);
                if (!this.CanReportOnCarry)
                {
                    this.CarringBody.GetComponentInChildren<BoxCollider2D>().enabled = true;
                }
                this.CarringBody = null;
            }
        }
    }
}
