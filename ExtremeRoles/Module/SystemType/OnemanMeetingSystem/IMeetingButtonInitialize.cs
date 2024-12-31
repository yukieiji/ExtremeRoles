using UnityEngine;

using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

public interface IMeetingButtonInitialize
{
	public void InitializeButon(Vector3 origin, Vector2 offset, PlayerVoteArea[] pvas);
}
