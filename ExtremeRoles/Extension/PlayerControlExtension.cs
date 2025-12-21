#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace ExtremeRoles.Extension.Player;

public static class PlayerControlExtension
{
	public static void MurderPlayer(this PlayerControl source, PlayerControl target)
	{
		source.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
	}

	public static bool IsValid([NotNullWhen(true)] this PlayerControl? @this)
		=>
			@this != null &&
			@this.Data != null &&
			!@this.Data.IsDead &&
			!@this.Data.Disconnected;

	public static bool IsValid([NotNullWhen(true)] this NetworkedPlayerInfo? @this)
		=>
			@this != null &&
			!@this.IsDead &&
			!@this.Disconnected &&
			@this.Object != null;

	public static bool IsInValid([NotNullWhen(false)] this NetworkedPlayerInfo? @this)
		=>
			@this == null ||
			@this.Object == null ||
			@this.IsDead ||
			@this.Disconnected;

	public static bool IsInValid([NotNullWhen(false)] this PlayerControl? @this)
		=>
			@this == null ||
			@this.Data == null ||
			@this.Data.IsDead ||
			@this.Data.Disconnected;

}
