using ExtremeRoles.Roles.API.Interface.Status;
using System.Collections.Generic;

namespace ExtremeRoles.Roles.Solo.Neutral.Missionary;

public sealed class MissionaryStatus : IStatusModel
{
	private readonly HashSet<byte> _judgementTarget = new HashSet<byte>(1);


	public void RemoveJudgementTarget(byte playerId)
		=> this._judgementTarget.Remove(playerId);

	public void AddJudgementTarget(byte playerId)
		=> this._judgementTarget.Add(playerId);

	public bool ContainsJudgementTarget(byte playerId)
		=> this._judgementTarget.Contains(playerId);

	public int JudgementTargetSize()
		=> this._judgementTarget.Count;
}
