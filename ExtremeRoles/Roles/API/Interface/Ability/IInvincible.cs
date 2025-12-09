namespace ExtremeRoles.Roles.API.Interface.Ability;

public interface IInvincible
{
	// nullは特殊(いわゆるレイダーの爆発の自決等の設定)
	public bool IsBlockKillFrom(byte? fromPlayer);

	public bool IsValidKillFromSource(byte source);

	public bool IsValidAbilitySource(byte source);
}
