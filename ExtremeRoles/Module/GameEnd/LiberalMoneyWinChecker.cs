using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.GameEnd;

public sealed class LiberalMoneyWinChecker(LiberalMoneyBankSystem system) : IGameEndChecker
{
	private readonly LiberalMoneyBankSystem bankSystem = system;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = (GameOverReason)RoleGameOverReason.LiberalRevolution;
		return this.bankSystem.Money > this.bankSystem.WinMoney;
	}
}
