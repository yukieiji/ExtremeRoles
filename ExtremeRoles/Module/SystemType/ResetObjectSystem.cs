using ExtremeRoles.Module.Interface;

using Hazel;

using System.Collections.Generic;

namespace ExtremeRoles.Module.SystemType;

public sealed class ResetObjectSystem : IExtremeSystemType
{
	private readonly List<IMeetingResetObject> resetObject = new List<IMeetingResetObject>();

	public void Add(IMeetingResetObject obj)
	{
		this.resetObject.Add(obj);
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart)
		{
			clearMeetingResetObject();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{ }

	private void clearMeetingResetObject()
	{
		foreach (var clerObject in this.resetObject)
		{
			if (clerObject == null) { continue; }
			clerObject.Clear();
		}
		this.resetObject.Clear();
	}
}
