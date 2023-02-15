using System.Collections.Generic;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Refacted.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;


namespace ExtremeRoles.GhostRoles.Impostor
{
    public sealed class SaboEvil : GhostRoleBase
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
            var sabSystem = CachedShipStatus.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>();
            if (sabSystem != null)
            {
                sabSystem.Timer = 0.0f;
            }
        }

        public override void CreateAbility()
        {
            this.Button = new AbilityCountButton(
                AbilityType.SaboEvilResetSabotageCool,
                this.UseAbility,
                this.isPreCheck,
                this.isAbilityUse,
                FastDestroyableSingleton<HudManager>.Instance.SabotageButton.graphic.sprite,
                rpcHostCallAbility: abilityCall);
            this.ButtonInit();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        { }

        protected override void OnMeetingEndHook()
        {
            return;
        }

        protected override void OnMeetingStartHook()
        {
            return;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateCountButtonOption(
                parentOps, 3, 20);
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller)
        { }

        private bool isPreCheck() => this.IsCommonUse();

        private bool isAbilityUse() => this.IsCommonUse();
        
        private void abilityCall()
        {
            ResetCool();
        }
    }
}
