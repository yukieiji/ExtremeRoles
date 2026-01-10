using ExtremeRoles.Core.Abstract;
using InfoController = ExtremeRoles.Core.Service.InfoOverlay.Controller;

namespace ExtremeRoles.Core.Infrastructure.Event;

public sealed class EventUpdator : ISubscriber
{
	public bool Invoke()
	{
		InfoController.Instance.UpdateOnEvent();
		return true;
	}
}
