using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Extension.Json;


#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class TeroristTeroSabotageSystem : IDeterioratableExtremeSystemType
{
	public enum Ops
	{
		Setup,
		Cancel
	}

	public bool IsDirty { get; private set; }
	public float ExplosionTimer { get; private set; }

	private readonly Dictionary<byte, int> setBomb = new Dictionary<byte, int>();
	private readonly HashSet<int> setedId = new HashSet<int>();
	private readonly float bombTimer;
	private readonly int setNum;

	private bool isActive = false;
	private float syncTimer = 0.0f;

	private JObject? json;

	public TeroristTeroSabotageSystem(float bombTimer, int setNum)
	{
		this.bombTimer = bombTimer;
		this.setNum = setNum;
	}

	public void Deteriorate(float deltaTime)
	{
		if (MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			!this.isActive)
		{
			this.flashActiveTo(false);
			return;
		}

		// タスク追加処理
		/*
		if (!PlayerTask.PlayerHasTaskOfType<HeliCharlesTask>(PlayerControl.LocalPlayer))
		{
			PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.HeliSabotage);
		}
		*/
		this.ExplosionTimer -= deltaTime;
		this.syncTimer -= deltaTime;

		if (this.syncTimer < 0f)
		{
			resetSyncTimer();
			this.IsDirty = true;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int newCount = reader.ReadPackedInt32();

		var newBombState = new HashSet<byte>();
		for (int i = 0; i < newCount; ++i)
		{
			byte bombId = reader.ReadByte();
			newBombState.Add(bombId);
			if (!this.setBomb.ContainsKey(bombId))
			{
				setBombToRandomPos(1);
			}
		}
		this.ExplosionTimer = reader.ReadSingle();

		List<byte> removeIndex = new List<byte>(this.setBomb.Count);
		foreach (byte id in this.setBomb.Keys)
		{
			if (!newBombState.Remove(id))
			{
				removeIndex.Add(id);
			}
		}
		foreach (byte id in removeIndex)
		{
			// 爆弾解除処理
		}
		checkAllCancel();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		int count = this.setBomb.Count;
		writer.WritePacked(count);
		foreach (byte bombId in this.setBomb.Keys)
		{
			writer.Write(bombId);
		}
		writer.Write(this.ExplosionTimer);
		resetSyncTimer();
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		// ホストのみ
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.Cancel:
				byte cancelBombId = msgReader.ReadByte();
				lock (this.setBomb)
				{
					if (this.setBomb.TryGetValue(cancelBombId, out int value))
					{
						// 解除処理
					}
				}
				checkAllCancel();
				this.IsDirty = true;
				break;
			case Ops.Setup:
				setBombToRandomPos(this.setNum);

				this.isActive = true;
				this.ExplosionTimer = this.bombTimer;
				this.IsDirty = true;

				break;
			default:
				return;
		}
	}

	private void checkAllCancel()
	{
		// 爆弾0 => サボ終了
		if (this.setBomb.Count == 0)
		{
			this.isActive = false;
			this.ExplosionTimer = 1000.0f;
			this.setedId.Clear();
			this.flashActiveTo(false);
		}
		else
		{
			this.flashActiveTo(true);
			this.isActive = true;
		}
	}

	// マップの設置箇所のIDを返す
	private List<VectorId> getSetPosIndex()
	{
		if (this.json == null)
		{
			this.json = JsonParser.GetJObjectFromAssembly(
				"ExtremeRoles.Resources.JsonData.ThiefTimePartPoint.json");
			if (this.json == null)
			{
				throw new ArgumentException("Json can't find");
			}

		}

		string key = GameSystem.CurMapKey;

		var result = new List<VectorId>(15);

		JArray posInfo = json.Get<JArray>(key);

		for (int i = 0; i < posInfo.Count; ++i)
		{
			JArray posArr = posInfo.Get<JArray>(i);

			result.Add(
				new VectorId(
					i, new Vector2(
						(float)(posArr[0]),
						(float)(posArr[1]))));
		}
		return result;
	}

	private void flashActiveTo(bool isActive)
	{

	}

	private void resetSyncTimer()
	{
		this.syncTimer = 1.0f;
	}

	private void setBombToRandomPos(int num)
	{
		var setPos = getSetPosIndex();
		setPos.RemoveAll(x => this.setedId.Contains(x.Id));

		var randomPos = setPos
			.OrderBy(x => RandomGenerator.Instance.Next())
			.Take(num);

		foreach (var pos in randomPos)
		{
			this.setedId.Add(pos.Id);
			// ボムの本体設置処理
		}
	}
}
