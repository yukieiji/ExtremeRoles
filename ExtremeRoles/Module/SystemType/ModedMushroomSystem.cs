using Hazel;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class ModedMushroomSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.ModedModedMushroom;

	public const string MushroomName = "ModdedMushroom";

	private Mushroom? prefab;
	private readonly Dictionary<int, Mushroom> modMushroom = new Dictionary<int, Mushroom>();
	private int id = 0;

	public enum Ops
	{
		Set,
		Scorp
	}

	public bool IsDirty => false;

	public static void RpcSetModMushroom(Vector2 pos)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type, (writer) =>
			{
				writer.Write((byte)Ops.Set);
				writer.Write(pos.x);
				writer.Write(pos.y);
			});
	}

	public static void RpcSporeModMushroom(int id)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type, (writer) =>
			{
				writer.Write((byte)Ops.Scorp);
				writer.WritePacked(id);
			});
	}

	public void Deteriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();

		switch (ops)
		{
			case Ops.Set:
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();
				setMushroom(new Vector2(x, y));
				break;
			case Ops.Scorp:
				int id = msgReader.ReadPackedInt32();
				sporeModMushroom(id);
				break;
			default:
				return;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	private void sporeModMushroom(int id)
	{
		if (MeetingHud.Instance == null ||
			AmongUsClient.Instance.IsGameOver ||
			!modMushroom.TryGetValue(id, out var mushroom) ||
			mushroom == null)
		{
			return;
		}
		mushroom.TriggerSpores();
	}

	private void setMushroom(Vector2 pos)
	{
		if (CachedShipStatus.Instance == null) { return; }

		if (prefab == null)
		{
			if (!CachedShipStatus.Instance.IsTryCast<FungleShipStatus>(out var ship))
			{
				var fungleAsset = AmongUsClient.Instance.ShipPrefabs[5];

				if (!fungleAsset.IsValid()) { return; }

				ship = fungleAsset
					.OperationHandle
					.Result
					.Cast<GameObject>()
					.GetComponent<FungleShipStatus>();
			}
			prefab = ship!.GetComponentInChildren<Mushroom>();
		}

		var newMushroom = Object.Instantiate(prefab, CachedShipStatus.Instance.transform);
		newMushroom.name = $"{MushroomName}_{id}";
		newMushroom.transform.localPosition = pos;
		modMushroom.Add(id, newMushroom);
		++id;
	}
}
