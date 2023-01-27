using ExtremeRoles.Module.RoleAssign;
using UnityEngine;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public GameObject Status => this.status;
        private GameObject status;

        public ExtremeShipStatus()
        {
            Initialize(false);
            this.playerVersion.Clear();
        }

        public void Initialize(
            bool includeGameObject = true)
        {
            // 以下リファクタ済み
            
            this.resetDeadPlayerInfo();
            this.resetGhostAbilityReport();
            this.resetGlobalAction();
            // this.resetPlayerSummary();
            this.resetMeetingCount();
            RoleAssignState.Instance.Reset();
            
            this.resetWins();

            this.ClearMeetingResetObject();

            if (!includeGameObject) { return; }

            if (this.status != null)
            {
                Object.Destroy(this.status);
                this.status = null;
                this.union = null;
            }
            this.status = new GameObject("ExtremeShipStatus");

            this.resetUpdateObject();
        }
    }
}
