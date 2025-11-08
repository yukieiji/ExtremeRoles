using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public interface IVoterValidtor
{
	public IEnumerable<byte> ValidPlayer { get; }
}
