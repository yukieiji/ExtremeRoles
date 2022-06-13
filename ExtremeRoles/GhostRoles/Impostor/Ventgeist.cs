using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.Impostor
{
    public class Ventgeist : GhostRoleBase
    {
        public enum Option
        {
            Range,
        }

        private float range;
        private Vent targetVent;

        public Ventgeist() : base(
            false,
            ExtremeRoleType.Impostor,
            ExtremeGhostRoleId.Ventgeist,
            ExtremeGhostRoleId.Ventgeist.ToString(),
            Palette.ImpostorRed)
        { }

        public static void VentAnime(int ventId)
        {
            RPCOperator.StartVentAnimation(ventId);
        }

        public override void CreateAbility()
        {
            this.Button = new AbilityCountButton(
                GhostRoleAbilityManager.AbilityType.VentgeistVentAnime,
                this.UseAbility,
                this.isPreCheck,
                this.isAbilityUse,
                HudManager.Instance.ImpostorVentButton.graphic.sprite,
                this.DefaultButtonOffset,
                rpcHostCallAbility: abilityCall);
            this.ButtonInit();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        {
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(Option.Range)].GetValue();
        }

        public override void ReseOnMeetingEnd()
        {
            return;
        }

        public override void ReseOnMeetingStart()
        {
            this.targetVent = null;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CreateFloatOption(
                Option.Range, 1.0f,
                0.2f, 3.0f, 0.1f,
                parentOps);
            CreateCountButtonOption(
                parentOps, 2, 10);
        }

        protected override void UseAbility(MessageWriter writer)
        {
            writer.Write(targetVent.Id);
        }

        private bool isPreCheck() => this.targetVent != null;

        private bool isAbilityUse()
        {
            this.targetVent = null;

            if (ShipStatus.Instance == null ||
                !ShipStatus.Instance.enabled) { return false; }

            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

            foreach (Vent vent in ShipStatus.Instance.AllVents)
            {
                if (vent == null) { continue; }
                if (ExtremeRolesPlugin.GameDataStore.CustomVent.IsCustomVent(vent.Id) &&
                    !vent.gameObject.active)
                {
                    continue;
                }
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= this.range)
                {
                    this.targetVent = vent;
                    break;
                }
            }

            return this.IsCommonUse() && this.targetVent != null;
        }
        private void abilityCall()
        {
            RPCOperator.StartVentAnimation(this.targetVent.Id);
            this.targetVent = null;
        }
    }
}
