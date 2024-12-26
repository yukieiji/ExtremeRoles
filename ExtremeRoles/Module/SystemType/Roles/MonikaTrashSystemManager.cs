
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;


#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MonikaTrashSystem : IDirtableSystemType
{
	public bool IsDirty => false;

	private readonly HashSet<byte> trash = new HashSet<byte>();
	private readonly Dictionary<byte, PlayerControl> trashPc = new Dictionary<byte, PlayerControl>();
	private readonly PlayerShowSystem showSystem = PlayerShowSystem.Get();

	public static bool TryGet([NotNullWhen(true)] out MonikaTrashSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.MonikaTrashSystem, out system);

	public static bool InvalidTarget(SingleRoleBase targetRole, byte sourcePlayerId)
		=> targetRole.Id is ExtremeRoleId.Monika &&
			TryGet(out var system) &&
			system.InvalidPlayer(sourcePlayerId);

	public void Deteriorate(float deltaTime)
	{
		// 勝利判定ちぇぇええく

		var removed = new HashSet<byte>();

		foreach (byte id in this.trash)
		{
			if (!this.trashPc.TryGetValue(id, out var targetPlayer) ||
				targetPlayer == null ||
				targetPlayer.Data == null ||
				targetPlayer.Data.Disconnected)
			{
				removed.Add(id);
				continue;
			}
			if (!this.showSystem.IsHide(targetPlayer))
			{
				this.showSystem.Hide(targetPlayer);
			}
		}

		foreach (byte id in removed)
		{
			this.trash.Remove(id);
			if (this.trashPc.TryGetValue(id, out var targetPlayer) &&
				this.showSystem.IsHide(targetPlayer))
			{
				this.showSystem.Show(targetPlayer);
			}
			this.trashPc.Remove(id);
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.ExiledEnd)
		{
			return;
		}
		foreach (byte id in this.trash)
		{
			var targetPlayer = Player.GetPlayerControlById(id);
			if (targetPlayer == null ||
				targetPlayer.Data == null ||
				targetPlayer.Data.IsDead ||
				targetPlayer.Data.Disconnected)
			{
				continue;
			}
			this.showSystem.Hide(targetPlayer);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte target = msgReader.ReadByte();
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		if (role.Id is not ExtremeRoleId.Monika)
		{
			return;
		}

		this.trash.Add(target);

		var targetPlayer = Player.GetPlayerControlById(target);
		if (targetPlayer != null)
		{
			this.showSystem.Hide(targetPlayer);
			this.trashPc.Add(target, targetPlayer);
		}
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
