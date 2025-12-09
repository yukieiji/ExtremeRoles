using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public interface IVoterShiftor
{
	public void Shift(Vector3 origin, Vector2 offset, PlayerVoteArea[] pvas);
}
