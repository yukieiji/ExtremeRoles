namespace ExtremeRoles.Module.ExtremeShipStatus;

public sealed partial class ExtremeShipStatus
{
	public ExtremeShipStatus()
	{
		Initialize();
		this.playerVersion.Clear();
	}

	public void Initialize()
	{
		// 以下リファクタ済み

		this.resetDeadPlayerInfo();
		this.resetGlobalAction();

		this.resetWins();
	}
}
