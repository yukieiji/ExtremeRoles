#nullable enable

namespace ExtremeRoles.Extension.Player;

public static class PlayerControlExtension
{
	public static void MurderPlayer(this PlayerControl source, PlayerControl target)
	{
		source.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
	}
	public static bool IsValid(this PlayerControl? @this)
		=>
			@this != null &&
			@this.Data != null &&
			!@this.Data.IsDead &&
			!@this.Data.Disconnected;

}
