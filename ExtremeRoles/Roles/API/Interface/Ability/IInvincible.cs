namespace ExtremeRoles.Roles.API.Interface.Ability;

public interface IInvincible
{
	// nullは特殊(いわゆるレイダーの爆発の自決等の設定)
	public bool IsBlockKillFrom(byte? fromPlayer);

	public bool IsValidTarget(byte target);
}
