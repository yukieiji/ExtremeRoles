using Hazel;
using ExtremeRoles.Core.Abstract;

namespace ExtremeRoles.Core.Service.SystemType;

public sealed class MeetingCountSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.MeetingCount;

	public int Counter { get; private set; } = 0;

	public void Increse()
	{
		this.Counter++;
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{ }
}
