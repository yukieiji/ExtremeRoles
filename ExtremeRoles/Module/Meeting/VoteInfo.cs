namespace ExtremeRoles.Module.Meeting;

public readonly record struct VoteInfo(byte VoterId, byte TargetId, int Count);
