#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Extension.Player;

public static class PlayerControlExtension
{
	public static void MurderPlayer(this PlayerControl source, PlayerControl target)
	{
		source.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
	}

	public static bool IsAlive([NotNullWhen(true)] this PlayerControl? @this)
		=> !@this.IsInvalid();

	public static bool IsAlive([NotNullWhen(true)] this NetworkedPlayerInfo? @this)
		=> !@this.IsInvalid();


	public static bool IsDead([NotNullWhen(true)] this PlayerControl? @this)
		=> @this != null && @this.Data.IsDead();

	public static bool IsDead([NotNullWhen(true)] this NetworkedPlayerInfo? @this)
		=>	@this != null &&
			@this.Object != null &&
			@this.IsDead;

	public static bool IsInvalid([NotNullWhen(false)] this NetworkedPlayerInfo? @this)
		=>
			@this == null ||
			@this.Object == null ||
			@this.IsDead ||
			@this.Disconnected;

	public static bool IsInvalid([NotNullWhen(false)] this PlayerControl? @this)
		=> @this == null || @this.Data.IsInvalid();

}
