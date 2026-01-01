using System;
using System.Collections.Generic;

using Hazel;
using TMPro;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class InspectorInspectSystem(InspectorInspectSystem.InspectMode mode) : IDirtableSystemType
{
	[Flags]
	public enum InspectMode
	{
		None = 0,
		Sabotage = 1 << 0,
		Vent = 1 << 1,
		Ability = 1 << 2,
	}

	public enum Ops
	{
		StartInspect,
		Add,
		EndInspect,
	}

	private readonly InspectMode mode = mode;
	private readonly Dictionary<byte, TargetPlayerContainer> allTarget = [];
	private TextMeshPro? text;

	public bool IsDirty => false;
	public const ExtremeSystemType Type = ExtremeSystemType.InspectorInspect;

	private sealed class TargetPlayerContainer
	{
		public PlayerControl Target
		{
			set
			{
				byte playerId = value.PlayerId;
				if (this.targetPlayer.ContainsKey(playerId))
				{
					return;
				}
				this.targetPlayer[playerId] = value;
				this.targetArrow[playerId] = new Arrow(Color.white);

			}
		}
		private readonly Dictionary<byte, PlayerControl> targetPlayer = [];
		private readonly Dictionary<byte, Arrow> targetArrow = [];

		public bool Contain(PlayerControl target)
			=> target != null && this.targetPlayer.ContainsKey(target.PlayerId);

		public void Update()
		{
			foreach (var (id, player) in this.targetPlayer)
			{
				if (this.targetArrow.TryGetValue(id, out var arrow) &&
					arrow is not null)
				{
					arrow.UpdateTarget(player.GetTruePosition());
				}
			}
		}

		public void Clear()
		{
			foreach (var arrow in this.targetArrow.Values)
			{
				arrow.Clear();
			}
			this.targetArrow.Clear();
			this.targetPlayer.Clear();
		}
	}

	public static void InspectAbility()
	{
		checkInspectToLocalPlayer(InspectMode.Ability);
	}
	public static void InspectVent()
	{
		checkInspectToLocalPlayer(InspectMode.Vent);
	}

	public static void InspectSabotage()
	{
		checkInspectToLocalPlayer(InspectMode.Sabotage);
	}

	private static void checkInspectToLocalPlayer(InspectMode mode)
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null)
		{
			return;
		}

		if (!(
				ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(Type, out var system) &&
				system.allTarget.Count > 0 &&
				system.mode.HasFlag(mode)
			))
		{
			return;
		}

		ExtremeSystemTypeManager.RpcUpdateSystem(Type, x =>
		{
			x.Write((byte)Ops.Add);
			x.Write(local.PlayerId);
		});
	}

	public void Deteriorate(float deltaTime)
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null ||
			this.allTarget.Count == 0)
		{
			if (this.text != null)
			{
				this.text.gameObject.SetActive(true);
			}
			return;
		}

		if (this.text == null)
		{
			this.text = UnityObject.Instantiate(
				Prefab.Text,
				Camera.main.transform, false);
			this.text.transform.localPosition = new Vector3(0.0f, -0.25f, -250.0f);
			this.text.enableWordWrapping = false;
			this.text.gameObject.SetActive(true);
		}

		// 役職本人の矢印表示処理
		if (this.allTarget.TryGetValue(local.PlayerId, out var rolePlayerShowTarget))
		{
			rolePlayerShowTarget.Update();
		}

		// インスペクトのターゲットへの警告表示
		foreach (var target in this.allTarget.Values)
		{
			if (target.Contain(local))
			{
				this.text.text = "「インスペクター」に自分の位置がバレた";
				return;
			}
		}
		// TODO: 能力発動時のテキストを整備する
		this.text.text = "〇〇が監視されている・・・・";
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart)
		{
			clear();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.StartInspect:
				startInspect(player.PlayerId);
				break;
			case Ops.Add:
				byte target = msgReader.ReadByte();
				addTarget(target);
				break;
			case Ops.EndInspect:
				endInspect(player.PlayerId);
				break;
		}
	}

	private void addTarget(byte targetId)
	{
		var targetPc = Player.GetPlayerControlById(targetId);
		if (targetPc == null)
		{
			return;
		}

		lock (this.allTarget)
		{
			foreach (var (id, target) in this.allTarget)
			{
				if (id != targetId)
				{
					target.Target = targetPc;
				}
			}
		}
	}

	private void startInspect(byte start)
	{
		lock (this.allTarget)
		{
			if (this.allTarget.TryGetValue(start, out var target) ||
				target is not null)
			{
				return;
			}
			this.allTarget[start] = new TargetPlayerContainer();
		}
	}

	private void endInspect(byte remove)
	{
		lock (this.allTarget)
		{
			if (!this.allTarget.TryGetValue(remove, out var target) ||
				target is null)
			{
				return;
			}
			target.Clear();
			this.allTarget.Remove(remove);
		}
	}
	private void clear()
	{
		foreach (var target in this.allTarget.Values)
		{
			target.Clear();
		}
		this.allTarget.Clear();
	}

	public void MarkClean()
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
