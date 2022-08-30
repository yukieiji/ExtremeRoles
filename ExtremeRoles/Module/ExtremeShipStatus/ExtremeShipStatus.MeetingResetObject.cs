using System.Collections.Generic;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        private List<IMeetingResetObject> resetObject = new List<IMeetingResetObject>();

        public void AddMeetingResetObject(IMeetingResetObject resetObject)
        {
            this.resetObject.Add(resetObject);
        }
        public void ClearMeetingResetObject()
        {
            foreach (IMeetingResetObject clerObject in this.resetObject)
            {
                clerObject.Clear();
            }
            this.resetObject.Clear();
        }
    }
}
