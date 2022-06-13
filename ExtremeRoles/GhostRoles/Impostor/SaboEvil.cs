using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;
using System.Collections.Generic;

namespace ExtremeRoles.GhostRoles.Impostor
{
    public class SaboEvil : GhostRoleBase
    {

        public SaboEvil() : base(
            false,
            ExtremeRoleType.Impostor,
            ExtremeGhostRoleId.SaboEvil,
            ExtremeGhostRoleId.SaboEvil.ToString(),
            Palette.ImpostorRed)
        { }

        public static void ResetCool()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>();
            if (sabSystem != null)
            {
                sabSystem.Timer = 0.0f;
            }
        }

        public override void CreateAbility()
        {
            this.Button = new AbilityCountButton(
                GhostRoleAbilityManager.AbilityType.SaboEvilResetSabotageCool,
                this.UseAbility,
                this.isPreCheck,
                this.isAbilityUse,
                HudManager.Instance.SabotageButton.graphic.sprite,
                this.DefaultButtonOffset,
                rpcHostCallAbility: abilityCall);
            this.ButtonInit();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        { }

        public override void ReseOnMeetingEnd()
        {
            return;
        }

        public override void ReseOnMeetingStart()
        {
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CreateCountButtonOption(
                parentOps, 3, 20);
        }

        protected override void UseAbility(MessageWriter writer)
        { }

        private bool isPreCheck() => this.IsCommonUse();

        private bool isAbilityUse() => this.IsCommonUse();
        
        private void abilityCall()
        {
            ResetCool();
        }
    }
}
