using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class DarkpointerDarknessSystem(float range, IUnityObjectFactory factory) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.DarkpointerDarkness;

	public enum Ops : byte
	{
		PlaceDarkness,
		RemoveDarkness,
	}

	public sealed class DarkCircle
	{
		public Vector2 Position => this.visualObject.transform.position;

		private readonly GameObject visualObject;

		public DarkCircle(PlayerControl player, float rnage, IUnityObjectFactory factory)
		{
			this.visualObject = factory.CreateGameObject($"DarkCircle_{player.PlayerId}");
			this.visualObject.transform.SetParent(player.transform);
			this.visualObject.transform.localPosition = new Vector3(0.0f, 0.0f, -20.0f);

			var rend = this.visualObject.AddComponent<SpriteRenderer>();
			rend.sprite = factory.LoadRoleSprite(ExtremeRoleId.Darkpointer, "DarkImg");
			this.visualObject.transform.localScale = new Vector3(rnage, rnage, 1f);
		}

		public void Remove()
		{
			if (this.visualObject != null)
			{
				UnityEngine.Object.Destroy(this.visualObject);
			}
		}
	}

	private readonly float range = range;
	private readonly IUnityObjectFactory unityObjectFactory = factory;
	private readonly Dictionary<byte, DarkCircle> activeCircles = new();
	private readonly ButtonLockSystem locker = ButtonLockSystem.CreateOrGetReportButtonLock();

	public bool IsDirty => false;

	public DarkpointerDarknessSystem(float range) : this(range, new DefaultUnityObjectFactory())
	{ }

	public bool IsPlayerInDarkness(PlayerControl? player)
	{
		if (player.IsInValid())
		{
			return false;
		}

		Vector2 pos = player.GetTruePosition();
		foreach (var circle in this.activeCircles.Values)
		{
			var diff = pos - circle.Position;
			if (diff.magnitude <= this.range)
			{
				return true;
			}
		}
		return false;
	}

	public void PlaceDarkness(byte playerId)
	{
		if (!Player.TryGetPlayerControl(playerId, out var player))
		{
			return;
		}

		if (this.activeCircles.TryGetValue(playerId, out var oldCircle))
		{
			oldCircle.Remove();
		}

		var circle = new DarkCircle(player, this.range, this.unityObjectFactory);
		this.activeCircles[playerId] = circle;
	}

	public void RemoveDarkness(byte playerId)
	{
		if (this.activeCircles.TryGetValue(playerId, out var circle))
		{
			circle.Remove();
			this.activeCircles.Remove(playerId);
		}
	}

	public void RpcPlaceDarkness(byte playerId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Ops.PlaceDarkness);
			writer.Write(playerId);
		});
	}

	public void RpcRemoveDarkness(byte playerId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Ops.RemoveDarkness);
			writer.Write(playerId);
		});
	}

	public void Deteriorate(float deltaTime)
	{
		if (MeetingHud.Instance != null || ExileController.Instance != null)
		{
			return;
		}

		if (IsPlayerInDarkness(PlayerControl.LocalPlayer))
		{
			this.locker.Lock((int)ButtonLockSystem.ConditionId.Darkpointer);
		}
		else
		{
			this.locker.UnLock((int)ButtonLockSystem.ConditionId.Darkpointer);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart)
		{
			this.locker.UnLock((int)ButtonLockSystem.ConditionId.Darkpointer);
			foreach (var circle in this.activeCircles.Values)
			{
				circle.Remove();
			}
			this.activeCircles.Clear();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.PlaceDarkness:
				byte playerId = msgReader.ReadByte();
				PlaceDarkness(playerId);
				break;
			case Ops.RemoveDarkness:
				byte removePlayerId = msgReader.ReadByte();
				RemoveDarkness(removePlayerId);
				break;
		}
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
