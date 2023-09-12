using Hazel;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

// 死体の絵変更はここでは行わない
public sealed class ThiefMeetingTimeStealSystem : IExtremeSystemType
{
	private record VectorId(int Id, Vector2 pos);

	public enum Ops
	{
		PickUp,
		Set,
	}

	public bool IsDirty { get; set; }

	private int curActiveNum = 0;

	private const ExtremeSystemType meetingSystemType = ExtremeSystemType.MeetingTimeOffset;

	private readonly int setNum;
	private readonly int setTimeOffset;
	private readonly int pickUpTimeOffset;

	private readonly Dictionary<int, TimeParts> timeParts = new Dictionary<int, TimeParts>();
	private readonly MeetingTimeChangeSystem internalSystem;

	public ThiefMeetingTimeStealSystem(int setNum, int setTimeOffset, int pickUpTimeOffset)
	{
		this.setNum = setNum;
		this.setTimeOffset = setTimeOffset;
		this.pickUpTimeOffset = pickUpTimeOffset;

		if (!ExtremeSystemTypeManager.Instance.TryGet<MeetingTimeChangeSystem>(
				meetingSystemType, out var system) ||
			system is null)
		{
			system = new MeetingTimeChangeSystem();
			ExtremeSystemTypeManager.Instance.TryAdd(meetingSystemType, system);
		}

		this.internalSystem = system;
		this.curActiveNum = 0;
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
				var posId = getSetPosIndex().FirstOrDefault(x => x.Id == id);
				if (posId is null) { continue; }
				setPart(posId);
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
			var part = this.timeParts[id];
			this.timeParts.Remove(id);
			Object.Destroy(part.gameObject);
		}
	}

	public void Detoriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingEnd &&
			AmongUsClient.Instance.AmHost &&
			this.curActiveNum >= 1)
		{
			for (int i = this.curActiveNum; i > 0; --i)
			{
				setPartToRandomPos();
			}
			this.curActiveNum = 0;
			this.IsDirty = true;
		}
	}

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
				this.curActiveNum++;
				changeMeetingTimeOffsetValue(this.setTimeOffset);
				break;
			case Ops.PickUp:
				changeMeetingTimeOffsetValue(this.pickUpTimeOffset);
				int picUpId = msgReader.ReadInt32();
				lock (this.timeParts)
				{
					if (this.timeParts.TryGetValue(picUpId, out var value))
					{
						this.timeParts.Remove(picUpId);
						Object.Destroy(value.gameObject);
					}
				}
				this.IsDirty = true;
				break;
			default:
				return;
		}
	}

	private void changeMeetingTimeOffsetValue(int value)
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			meetingSystemType,
			(x) =>
			{
				x.Write((byte)MeetingTimeChangeSystem.Ops.ChangeMeetingHudPermOffset);
				x.WritePacked(this.internalSystem.PermOffset + value);
			});
	}

	private void setPartToRandomPos()
	{
		var setPos = getSetPosIndex();
		setPos.RemoveAll(x => !this.timeParts.ContainsKey(x.Id));

		var randomPos = setPos.OrderBy(x => RandomGenerator.Instance.Next()).ToList();

		int setNum = 0;
		while (
			setNum < this.setNum &&
			randomPos.Count > 0)
		{
			VectorId posId = randomPos[0];
			randomPos.RemoveAt(0);
			setPart(posId);
			setNum++;
		}
	}

	private void setPart(VectorId posId)
	{
		int id = posId.Id;
		Vector2 pos = posId.pos;
		GameObject obj = new GameObject($"TimePart_{id}");
		var part = obj.AddComponent<TimeParts>();
		part.SetTimeOffset(id);

		obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

		this.timeParts.Add(id, part);
	}

	// マップの設置箇所のIDを返す
	private static List<VectorId> getSetPosIndex()
		=> new List<VectorId>();
}
