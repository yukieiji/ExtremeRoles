using ExtremeRoles.GhostRoles;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        private GhostRoleAbilityReporter reporter = new GhostRoleAbilityReporter();

        public void AddGhostRoleAbilityReport(AbilityType type)
        {
            this.reporter.AddAbilityCall(type);
        }

        public string GetGhostAbilityReport() => this.reporter.CreateReport();

        public void resetGhostAbilityReport()
        {
            this.reporter.Clear();
        }
    }
}
