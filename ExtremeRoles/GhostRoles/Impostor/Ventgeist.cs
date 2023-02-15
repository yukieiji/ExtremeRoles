using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Extension.Ship;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Refacted.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;


namespace ExtremeRoles.GhostRoles.Impostor
{
    public sealed class Ventgeist : GhostRoleBase
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
                AbilityType.VentgeistVentAnime,
                this.UseAbility,
                this.isPreCheck,
                this.isAbilityUse,
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.graphic.sprite,
                rpcHostCallAbility: abilityCall);
            this.ButtonInit();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        {
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(Option.Range)].GetValue();
        }

        protected override void OnMeetingEndHook()
        {
            return;
        }

        protected override void OnMeetingStartHook()
        {
            this.targetVent = null;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateFloatOption(
                Option.Range, 1.0f,
                0.2f, 3.0f, 0.1f,
                parentOps);
            CreateCountButtonOption(
                parentOps, 2, 10);
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller)
        {
            caller.WriteInt(targetVent.Id);
        }

        private bool isPreCheck() => this.targetVent != null;

        private bool isAbilityUse()
        {
            this.targetVent = null;

            ShipStatus ship = CachedShipStatus.Instance;

            if (ship == null ||
                !ship.enabled) { return false; }

            Vector2 truePosition = CachedPlayerControl.LocalPlayer.PlayerControl.GetTruePosition();

            foreach (Vent vent in ship.AllVents)
            {
                if (vent == null) { continue; }
                if (ship.IsCustomVent(vent.Id) &&
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
