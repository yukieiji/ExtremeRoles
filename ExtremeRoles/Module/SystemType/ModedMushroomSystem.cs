﻿using Hazel;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public sealed class ModedMushroomSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.ModedMushroom;

	public const string MushroomName = "ModdedMushroom";

	private readonly Mushroom prefab;
	private readonly Dictionary<int, Mushroom> modMushroom = new Dictionary<int, Mushroom>();

	private readonly float delaySecond;
	private int id = 0;

	public enum Ops
	{
		Set,
		Scorp
	}

	public ModedMushroomSystem(float delaySecond)
	{
		this.delaySecond = delaySecond;

		// ファングル持ってくる
		ShipStatus ship = GameSystem.GetShipObj(5);
		var mushroom = ship.GetComponentInChildren<Mushroom>();
		this.prefab = Object.Instantiate(
			mushroom, PlayerControl.LocalPlayer.transform);
		this.prefab.gameObject.SetActive(false);
	}

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

	private void sporeModMushroom(int id)
	{
		if (MeetingHud.Instance != null ||
			AmongUsClient.Instance.IsGameOver ||
			!this.modMushroom.TryGetValue(id, out var mushroom) ||
			mushroom == null)
		{
			return;
		}
		mushroom.TriggerSpores();
	}

	private void setMushroom(Vector2 pos)
	{
		if (ShipStatus.Instance == null) { return; }

		var newMushroom = Object.Instantiate(prefab, ShipStatus.Instance.transform);
		newMushroom.gameObject.SetActive(true);
		newMushroom.name = $"{MushroomName}_{this.id}";

		var setPos = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

		newMushroom.transform.position = setPos;
		newMushroom.origPosition = setPos;

		var enabler = newMushroom.gameObject.AddComponent<DelayableEnabler>();
		enabler.Initialize(newMushroom, this.delaySecond);

		this.modMushroom.Add(this.id, newMushroom);
		++this.id;
	}
}
