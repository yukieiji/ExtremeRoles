using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.InfoOverlay;

public sealed class EventUpdator : ISubscriber
{
	public bool Invoke()
	{
		Controller.Instance.UpdateOnEvent();
		return true;
	}
}
