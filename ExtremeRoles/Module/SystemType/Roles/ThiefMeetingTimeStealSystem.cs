using Hazel;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

// 死体の絵変更はここでは行わない
public sealed class ThiefMeetingTimeStealSystem : IExtremeSystemType
{
	public enum Ops
	{
		PickUp,
		Set,
	}

	public bool IsDirty { get; set; }
	private const ExtremeSystemType meetingSystemType = ExtremeSystemType.MeetingTimeOffset;

	private readonly int setNum;
	private readonly float setTimeOffset;
	private readonly float pickUpTimeOffset;

	private readonly Dictionary<int, GameObject> timeParts = new Dictionary<int, GameObject>();
	private readonly MeetingTimeOffsetSystem internalSystem;

	public ThiefMeetingTimeStealSystem(int setNum, float setTimeOffset, float pickUpTimeOffset)
	{
		this.setNum = setNum;
		this.setTimeOffset = setTimeOffset;
		this.pickUpTimeOffset = pickUpTimeOffset;

		if (!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeOffsetSystem>(
				meetingSystemType, out var system) ||
			system is null)
		{
			system = new MeetingTimeOffsetSystem();
			ExtremeSystemTypeManager.Instance.TryAdd(meetingSystemType, system);
		}

		this.internalSystem = system;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int newNum = reader.ReadPackedInt32();
		var hostId = new HashSet<int>();
		for (int i = 0; i < newNum; ++i)
		{
			int id = reader.ReadPackedInt32();
			hostId.Add(id);
			if (!this.timeParts.ContainsKey(id))
			{
				// オブジェクト設置
				setPart(id);
			}
		}

		List<int> removeIndex = new List<int>(this.timeParts.Count);
		foreach (int id in this.timeParts.Keys)
		{
			if (!hostId.Remove(id))
			{
				removeIndex.Add(id);
			}
		}
		foreach (int id in removeIndex)
		{
			// 削除処理
			this.timeParts.Remove(id);
		}
	}

	public void Detoriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.timeParts.Count);
		foreach (int id in this.timeParts.Keys)
		{
			writer.WritePacked(id);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		// ホストのみ
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.Set:
				changeMeetingTimeOffsetValue(this.setTimeOffset);
				setPartToRandomPos();
				break;
			case Ops.PickUp:
				changeMeetingTimeOffsetValue(this.pickUpTimeOffset);
				int picUpId = msgReader.ReadInt32();
				lock (this.timeParts)
				{
					if (this.timeParts.TryGetValue(picUpId, out var value))
					{
						this.timeParts.Remove(picUpId);
						// 削除処理
					}
				}
				break;
			default:
				return;
		}
		this.IsDirty = true;
	}

	private void changeMeetingTimeOffsetValue(float value)
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			meetingSystemType,
			(x) =>
			{
				x.Write((byte)MeetingTimeOffsetSystem.Ops.ChangeMeetingHudOffset);
				x.Write(this.internalSystem.MeetingHudTimerOffset + value);
			});
	}

	private void setPartToRandomPos()
	{
		var setPos = getSetPosIndex();
		setPos.RemoveAll(x => !this.timeParts.ContainsKey(x));

		var randomPos = setPos.OrderBy(x => RandomGenerator.Instance.Next()).ToList();

		int setNum = 0;
		while (
			setNum < this.setNum &&
			randomPos.Count > 0)
		{
			int id = randomPos[0];

			randomPos.RemoveAt(0);
			// 追加処理
			setPart(id);
		}
	}

	private void setPart(int id)
	{
		this.timeParts.Add(id, new GameObject());
	}

	// マップの設置箇所のIDを返す
	private static List<int> getSetPosIndex()
		=> new List<int>();
}
