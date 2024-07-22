using ExtremeRoles.Module.RoleAssign;
using UnityEngine;

namespace ExtremeRoles.Module.ExtremeShipStatus;

public sealed partial class ExtremeShipStatus
{
	public ExtremeShipStatus()
	{
		Initialize(false);
		this.playerVersion.Clear();
	}

	public void Initialize(
		bool includeGameObject = true)
	{
		// 以下リファクタ済み

		this.resetDeadPlayerInfo();
		this.resetGlobalAction();
		// this.resetPlayerSummary();
		this.resetMeetingCount();
		RoleAssignState.TryDestroy();

		this.resetWins();

		this.ClearMeetingResetObject();

		if (!includeGameObject) { return; }

	}
}
