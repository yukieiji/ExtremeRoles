using UnityEngine;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
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
            this.resetPlayerSummary();
            this.resetMeetingCount();
            this.resetRoleAssign();
            
            this.resetWins();

            this.ClearMeetingResetObject();
            this.ResetVison();

            if (!includeGameObject) { return; }

            if (this.status != null)
            {
                Object.Destroy(this.status);
                this.status = null;
                this.history = null;
                this.union = null;
            }
            this.status = new GameObject("ExtremeShipStatus");

            this.resetUpdateObject();
        }
    }
}
