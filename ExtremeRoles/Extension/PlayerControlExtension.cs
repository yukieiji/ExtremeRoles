namespace ExtremeRoles.Extension.Player;

public static class PlayerControlExtension
{
	public static void MurderPlayer(this PlayerControl source, PlayerControl target)
	{
		source.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
	}
}
