using Hazel;

using ExtremeRoles.Module.Interface;
using System.Collections.Generic;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral.Yandere;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class SurrogatorGurdSystem(float preventKillTime) : IDirtableSystemType
{
	private int guardNum = 0;
	private HashSet<byte> oneSideLovers = [];
	private bool init = false;

	public float PreventKillTime { get; } = preventKillTime;

	public enum Ops : byte
	{
		Add,
		Reduce
	}

	private const ExtremeSystemType systemType = ExtremeSystemType.SurrogatorGurdSystem;

	public bool IsDirty => false;

	public void Deteriorate(float deltaTime)
	{
		if (this.init ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			IntroCutscene.Instance != null)
		{
			return;
		}
		this.init = true;
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				!ExtremeRoleManager.TryGetSafeCastedRole<YandereRole>(player.PlayerId, out var yandere))
			{
				continue;
			}
			bool isNotNull = yandere.OneSidedLover != null;
			this.init &= isNotNull;
			if (isNotNull)
			{
				this.oneSideLovers.Add(yandere.OneSidedLover.PlayerId);
			}
		}
	}

	public void AddGuardNum(int num)
	{
		guardNum += num;
	}

	public static SurrogatorGurdSystem CreateOrGet(float preventKillTime)
		=> ExtremeSystemTypeManager.Instance.CreateOrGet(
				systemType, () => new SurrogatorGurdSystem(preventKillTime));

	public static void RpcAddNum()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			systemType, x =>
			{
				x.Write((byte)Ops.Add);
			});
	}

	public static void RpcReduce()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			systemType, x =>
			{
				x.Write((byte)Ops.Reduce);
			});
	}

	public bool CanGuard(byte playerId)
		=> this.oneSideLovers.Contains(playerId);

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{

	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		switch ((Ops)msgReader.ReadByte())
		{
			case Ops.Add:
				this.guardNum++;
				break;
			case Ops.Reduce:
				this.guardNum--;
				break;
			default:
				break;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
