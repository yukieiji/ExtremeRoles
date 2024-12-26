using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MonikaTrashSystem : IExtremeSystemType
{
	private readonly HashSet<byte> trash = new HashSet<byte>();

	public static bool TryGet([NotNullWhen(true)] out MonikaTrashSystem system)
	{

	}

	public static bool InvalidTarget(SingleRoleBase targetRole, byte sourcePlayerId)
		=> targetRole.Id is ExtremeRoleId.Monika &&
			TryGet(out var system) &&
			system.InvalidPlayer(sourcePlayerId);

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{

	}

	public bool CanChatBetween(NetworkedPlayerInfo source, NetworkedPlayerInfo local)
		=>
		(
			source.IsDead && local.IsDead //死んでる人to死んでる人
		)
		||
		(
			this.InvalidPlayer(source) && local.IsDead //ゴミ箱to死んでる人
		)
		||
		(
			this.InvalidPlayer(source) && this.InvalidPlayer(local) //ゴミ箱toゴミ箱
		)
		||
		(
			!source.IsDead && this.InvalidPlayer(local) //生存者toゴミ箱
		)
		||
		(
			!source.IsDead && !local.IsDead //生存者to生存者
		);

	public void InitializeButton(PlayerVoteArea[] buttons)
	{
		foreach (var pva in buttons)
		{
			if (pva == null ||
				pva.AmDead ||
				!InvalidPlayer(pva))
			{
				continue;
			}
			// モニカの色に変える pva.Background.color
			pva.XMark.gameObject.SetActive(true);
		}
	}

	public bool InvalidPlayer(byte playerId)
		=> this.trash.Contains(playerId);

	public bool InvalidPlayer(PlayerVoteArea pva)
		=> InvalidPlayer(pva.TargetPlayerId);

	public bool InvalidPlayer(PlayerControl player)
		=> this.trash.Contains(player.PlayerId);

	public bool InvalidPlayer(NetworkedPlayerInfo player)
		=> this.trash.Contains(player.PlayerId);

	public int GetVoteAreaOrder(PlayerVoteArea pva)
		=> InvalidPlayer(pva) ? 0 : 10;
}
