using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	private readonly IReadOnlyList<string> modeKey = createModeKey(mode);
	private readonly Dictionary<byte, TargetPlayerContainer> allTarget = [];
	private readonly StringBuilder builder = new StringBuilder();
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
				
				var arrow = new Arrow(ColorPalette.InspectorAmberYellow);
				arrow.SetActive(false);
				this.targetArrow[playerId] = arrow;

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
					arrow.SetActive(true);
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
				this.text.gameObject.SetActive(false);
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
		}
		this.text.gameObject.SetActive(true);

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
				this.text.text = Tr.GetString("inspectArrowWarning");
				return;
			}
		}

		this.text.text = this.createText();
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

	private string createText()
	{
		this.builder.Clear();
		this.builder.Append('「');
		this.builder.Append(string.Join(",", this.modeKey.Select(x => Tr.GetString(x))));
		this.builder.Append('」');
		this.builder.Append(Tr.GetString("inspectSystem"));

		return this.builder.ToString();
	}

	private static IReadOnlyList<string> createModeKey(InspectMode mode)
	{
		var parts = new List<string>();
		if (mode.HasFlag(InspectMode.Sabotage))
		{
			parts.Add("sabotageKey");
		}
		if (mode.HasFlag(InspectMode.Vent))
		{
			parts.Add("ventKey");
		}
		if (mode.HasFlag(InspectMode.Ability))
		{
			parts.Add("buttonAbility");
		}
		return parts;
	}
}
