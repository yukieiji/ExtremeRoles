using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ExtremeRoles.Module.Interface;
using Hazel;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MonikaTrashSystem : IExtremeSystemType
{
	private readonly HashSet<byte> trash = new HashSet<byte>();

	public static bool TryGet([NotNullWhen(true)] out MonikaTrashSystem system)
	{

	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{

	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{

	}

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

	public int GetVoteAreaOrder(PlayerVoteArea pva)
		=> InvalidPlayer(pva) ? 0 : 10;
}
