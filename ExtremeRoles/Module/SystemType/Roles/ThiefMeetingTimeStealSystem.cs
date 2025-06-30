using Hazel;

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Extension.Json;


#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class ThiefMeetingTimeStealSystem : IDirtableSystemType
{
	public enum Ops
	{
		PickUp,
		Set,
	}

	public bool IsDirty { get; set; } = false;

	private int curActiveNum = 0;

	private const ExtremeSystemType meetingSystemType = ExtremeSystemType.ModdedMeetingTimeSystem;

	private readonly int setNum;
	private readonly int setTimeOffset;
	private readonly int pickUpTimeOffset;

	private readonly Dictionary<int, TimeParts> timeParts = new Dictionary<int, TimeParts>();
	private readonly ModdedMeetingTimeSystem internalSystem;

	private static JObject? json = null;

	public ThiefMeetingTimeStealSystem(int setNum, int setTimeOffset, int pickUpTimeOffset)
	{
		this.setNum = setNum;
		this.setTimeOffset = setTimeOffset;
		this.pickUpTimeOffset = pickUpTimeOffset;

		this.internalSystem = ExtremeSystemTypeManager.Instance.CreateOrGet<ModdedMeetingTimeSystem>(
			meetingSystemType);
		this.curActiveNum = 0;
	}

	public void MarkClean()
	{
		this.IsDirty = false;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int newNum = reader.ReadPackedInt32();
		var hostId = new HashSet<int>();
		var pos = getSetPosIndex();
		for (int i = 0; i < newNum; ++i)
		{
			int id = reader.ReadPackedInt32();
			hostId.Add(id);
			if (!this.timeParts.ContainsKey(id))
			{
				// オブジェクト設置
				var posId = pos.FirstOrDefault(x => x.Id == id);
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
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		// ホストのみ
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.PickUp:
				changeMeetingTimeOffsetValue(this.pickUpTimeOffset);
				int picUpId = msgReader.ReadPackedInt32();
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
			case Ops.Set:
				this.curActiveNum++;
				changeMeetingTimeOffsetValue(this.setTimeOffset);
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
				x.Write((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingHudPermOffset);
				x.WritePacked(this.internalSystem.PermOffset + value);
			});
	}

	private void setPartToRandomPos()
	{
		var setPos = getSetPosIndex();
		setPos.RemoveAll(x => this.timeParts.ContainsKey(x.Id));

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
		Vector2 pos = posId.Pos;
		GameObject obj = new GameObject($"TimePart_{id}");
		var part = obj.AddComponent<TimeParts>();
		part.Id = id;

		obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

		this.timeParts.Add(id, part);
	}

	// マップの設置箇所のIDを返す
	private static List<VectorId> getSetPosIndex()
	{
		if (json == null)
		{
			json = JsonParser.GetJObjectFromAssembly(
				"ExtremeRoles.Resources.JsonData.ThiefTimePartPoint.json");
			if (json == null)
			{
				throw new System.ArgumentNullException("Json data is null!!!!");
			}
		}
		string key = Map.Name;

		var result = new List<VectorId>(10);

		JArray? posInfo = json.Get<JArray>(key);
		if (posInfo == null) { return result; }

		for (int i = 0; i < posInfo.Count; ++i)
		{
			JArray? id = posInfo.Get<JArray>(i);
			if (id == null) { continue; }

			result.Add(
				new VectorId(
					i, new Vector2(
						(float)(id[0]),
						(float)(id[1]))));
		}
		return result;
	}
}
