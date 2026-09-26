using System.Collections.Generic;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerStatusModel : IStatusModel, IStatusMovable
{
	public bool CanMove { get; set; } = true;
	public bool IsPurging { get; set; } = false;

	public float RobRange { get; set; } = 1.0f;
	public float RobActiveTime { get; set; } = 2.0f;
	public int ContactPlayerCount { get; set; } = 1;
	public float PurgeTime { get; set; } = 5.0f;

	public HashSet<byte> ExecutionTargets { get; } = new HashSet<byte>();
	public HashSet<byte> PendingReports { get; } = new HashSet<byte>();
	public Dictionary<byte, HashSet<byte>> TaskPhaseContacts { get; } = new Dictionary<byte, HashSet<byte>>();
}
