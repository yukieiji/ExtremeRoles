
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;
using Il2CppInterop.Generator.Extensions;




#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MonikaTrashSystem : IDirtableSystemType
{
	public bool IsDirty { get; set; } = false;
	private bool isEnd = false;
	private bool isMonikaAlive = true;

	public enum Ops
	{
		AddTrash,
		StartMeeting,
		ClearTrash,
	}

	private readonly HashSet<byte> trash = new HashSet<byte>();
	private readonly Dictionary<byte, PlayerControl> trashPc = new Dictionary<byte, PlayerControl>();
	private readonly PlayerShowSystem showSystem = PlayerShowSystem.Get();
	private readonly OnemanMeetingSystemManager meetingSystem = OnemanMeetingSystemManager.CreateOrGet();

	public static bool TryGet([NotNullWhen(true)] out MonikaTrashSystem? system)
		=> ExtremeSystemTypeManager.Instance.TryGet(ExtremeSystemType.MonikaTrashSystem, out system);

	public static bool InvalidTarget(SingleRoleBase targetRole, byte sourcePlayerId)
		=> targetRole.Id is ExtremeRoleId.Monika &&
			TryGet(out var system) &&
			system.InvalidPlayer(sourcePlayerId);

	public void Deteriorate(float deltaTime)
	{
		if (this.isMonikaAlive &&
			AmongUsClient.Instance.AmHost &&
			MeetingHud.Instance == null &&
			ExileController.Instance == null &&
			!this.isEnd)
		{
			checkMonikaSpecialMeeting();
		}

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
	{
		writer.WritePacked(this.trash.Count);
		foreach (int id in this.trash)
		{
			writer.Write(id);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			byte id = reader.ReadByte();
			// 会議が始まった時点でモニカの勝利は確定しているためPlayerControlは要らない
			this.trash.Add(id);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is not ResetTiming.ExiledEnd)
		{
			return;
		}
		foreach (byte id in this.trash)
		{
			if (!this.trashPc.TryGetValue(id, out var targetPlayer) ||
				targetPlayer == null ||
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
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.AddTrash:
				byte target = msgReader.ReadByte();
				this.addTrash(target);
				break;
			case Ops.StartMeeting:
				byte callerId = msgReader.ReadByte();
				var caller = Player.GetPlayerControlById(callerId);
				if (caller == null)
				{
					return;
				}
				this.meetingSystem.Start(caller, OnemanMeetingSystemManager.Type.Monika);
				break;
			case Ops.ClearTrash:
				clearTrash();
				break;
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

	private void addTrash(byte targetPlayerId)
	{
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		if (role.Id is not ExtremeRoleId.Monika)
		{
			return;
		}
		this.trash.Add(targetPlayerId);

		var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (targetPlayer != null)
		{
			this.showSystem.Hide(targetPlayer);
			this.trashPc.Add(targetPlayerId, targetPlayer);
		}
	}

	private void clearTrash()
	{
		this.trash.Clear();

		foreach (var pc in this.trashPc.Values)
		{
			if (this.showSystem.IsHide(pc))
			{
				this.showSystem.Show(pc);
			}
		}
		this.trashPc.Clear();
	}

	private void checkMonikaSpecialMeeting()
	{
		int aliveNum = 0;
		int monikaNum = 0;
		byte monikaId = byte.MaxValue;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null ||
				player.IsDead ||
				player.Disconnected ||
				this.InvalidPlayer(player))
			{
				continue;
			}
			if (ExtremeRoleManager.TryGetRole(player.PlayerId, out var role))
			{
				monikaNum++;
				monikaId = player.PlayerId;
			}
			aliveNum++;
		}

		if (monikaNum <= 0)
		{
			this.isMonikaAlive = false;
			this.trash.Clear();
			ExtremeSystemTypeManager.RpcUpdateSystem(
				ExtremeSystemType.MonikaTrashSystem,
				x =>
				{
					x.Write((byte)Ops.ClearTrash);
				});
			return;
		}

		if (monikaNum > 1 || aliveNum != 2)
		{
			return;
		}
		this.isEnd = true;
		// monikaのゴミ箱をシンクロ
		this.IsDirty = true;
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.MonikaTrashSystem,
			x =>
			{
				x.Write((byte)Ops.StartMeeting);
				x.Write(monikaId);
			});
	}
}
