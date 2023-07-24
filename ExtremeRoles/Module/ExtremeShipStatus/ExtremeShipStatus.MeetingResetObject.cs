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
			foreach (var clerObject in this.resetObject)
			{
				if (clerObject == null) { continue; }
				clerObject.Clear();
			}
			this.resetObject.Clear();
		}
	}
}
