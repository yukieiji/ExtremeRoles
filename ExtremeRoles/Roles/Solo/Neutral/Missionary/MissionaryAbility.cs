using System;

using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Solo.Neutral.Missionary;

public sealed class MissionaryAbility(
	bool isUseSolemnJudgment,
	int maxJudgementTarget,
	MissionaryStatus status) : IAbility, IVoteCheck
{
	private readonly MissionaryStatus _status = status;

	private readonly bool _isUseSolemnJudgment = isUseSolemnJudgment;
	private readonly int _maxJudgementTarget = maxJudgementTarget;

	public void VoteTo(byte target)
	{
		if (!_isUseSolemnJudgment ||
			target == 252 ||
			target == 253 ||
			target == 254 ||
			target == byte.MaxValue ||
			_status.JudgementTargetSize() > this._maxJudgementTarget) { return; }

		this._status.AddJudgementTarget(target);
	}
}
