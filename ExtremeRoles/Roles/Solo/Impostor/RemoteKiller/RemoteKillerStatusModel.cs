using System.Collections.Generic;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerStatusModel : IStatusModel, IStatusMovable
{
	public bool CanMove { get; set; } = true;

	public bool IsPurging { get; private set; } = false;

	public float RobRange { get; }
	public float RobActiveTime { get; }
	public int ContactPlayerCount { get; }
	public float PurgeTime { get; }

	public IReadOnlyCollection<byte> ExecutionTargets => this.executionTargets;
	public IReadOnlyCollection<byte> PendingReports => this.pendingReports;
	public IReadOnlyDictionary<byte, HashSet<byte>> TaskPhaseContacts => this.taskPhaseContacts;

	private readonly HashSet<byte> executionTargets = new HashSet<byte>();
	private readonly HashSet<byte> pendingReports = new HashSet<byte>();
	private readonly Dictionary<byte, HashSet<byte>> taskPhaseContacts = new Dictionary<byte, HashSet<byte>>();

	public RemoteKillerStatusModel(float robRange, float robActiveTime, int contactPlayerCount, float purgeTime)
	{
		this.RobRange = robRange;
		this.RobActiveTime = robActiveTime;
		this.ContactPlayerCount = contactPlayerCount;
		this.PurgeTime = purgeTime;
	}

	public void SetPurging(bool purging)
	{
		this.IsPurging = purging;
	}

	public bool HasExecutionTarget(byte targetId) => this.executionTargets.Contains(targetId);

	public void AddExecutionTarget(byte targetId)
	{
		this.executionTargets.Add(targetId);
		this.pendingReports.Add(targetId);
	}

	public void RemoveExecutionTarget(byte targetId)
	{
		this.executionTargets.Remove(targetId);
		this.pendingReports.Remove(targetId);
		this.taskPhaseContacts.Remove(targetId);
	}

	public void RemovePendingReport(byte targetId)
	{
		this.pendingReports.Remove(targetId);
	}

	public bool IsPendingReport(byte targetId) => this.pendingReports.Contains(targetId);

	public void ClearTaskPhaseContacts(byte targetId)
	{
		if (this.taskPhaseContacts.TryGetValue(targetId, out var set))
		{
			set.Clear();
		}
	}

	public void RecordContact(byte targetId, byte contactPlayerId)
	{
		if (!this.taskPhaseContacts.TryGetValue(targetId, out var set))
		{
			set = new HashSet<byte>();
			this.taskPhaseContacts[targetId] = set;
		}
		set.Add(contactPlayerId);
	}

	public void Reset()
	{
		this.CanMove = true;
		this.IsPurging = false;
		this.executionTargets.Clear();
		this.pendingReports.Clear();
		this.taskPhaseContacts.Clear();
	}
}
