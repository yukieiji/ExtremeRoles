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

public sealed class DarkpointerDarknessSystem : IExtremeSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.DarkpointerDarkness;

	public enum Ops : byte
	{
		PlaceDarkness,
		RemoveDarkness,
	}

	public sealed class DarkCircle
	{
		public Vector2 Position { get; }
		public float Range { get; }

		private GameObject? visualObject;

		public DarkCircle(byte playerId, Vector2 position, float range, IUnityObjectFactory factory)
		{
			this.Position = position;
			this.Range = range;

			createVisual(playerId, factory);
		}

		private void createVisual(byte playerId, IUnityObjectFactory factory)
		{
			this.visualObject = factory.CreateGameObject($"DarkCircle_{playerId}");
			this.visualObject.transform.position = new Vector3(
				this.Position.x,
				this.Position.y,
				this.Position.y / 1000f - 0.05f);

			var rend = this.visualObject.AddComponent<SpriteRenderer>();
			rend.sprite = factory.LoadRoleSprite(ExtremeRoleId.Darkpointer);
			rend.color = Color.black;
			rend.sortingOrder = 32767;

			float diameter = this.Range * 2f;
			this.visualObject.transform.localScale = new Vector3(diameter, diameter, 1f);
		}

		public void Remove()
		{
			if (this.visualObject != null)
			{
				UnityEngine.Object.Destroy(this.visualObject);
				this.visualObject = null;
			}
		}
	}

	public float Range { get; set; } = 2.5f;

	private readonly IUnityObjectFactory unityObjectFactory;
	private readonly Dictionary<byte, DarkCircle> activeCircles = new();

	public DarkpointerDarknessSystem() : this(new DefaultUnityObjectFactory())
	{ }

	public DarkpointerDarknessSystem(IUnityObjectFactory factory)
	{
		this.unityObjectFactory = factory;
	}

	public bool IsPlayerInDarkness(PlayerControl? player)
	{
		if (player.IsInValid())
		{
			return false;
		}

		Vector2 pos = player.GetTruePosition();
		lock (this.activeCircles)
		{
			foreach (var circle in this.activeCircles.Values)
			{
				if (Vector2.Distance(pos, circle.Position) <= circle.Range)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PlaceDarkness(byte playerId, Vector2 position)
	{
		lock (this.activeCircles)
		{
			if (this.activeCircles.TryGetValue(playerId, out var oldCircle))
			{
				oldCircle.Remove();
			}

			var circle = new DarkCircle(playerId, position, this.Range, this.unityObjectFactory);
			this.activeCircles[playerId] = circle;
		}
	}

	public void RemoveDarkness(byte playerId)
	{
		lock (this.activeCircles)
		{
			if (this.activeCircles.TryGetValue(playerId, out var circle))
			{
				circle.Remove();
				this.activeCircles.Remove(playerId);
			}
		}
	}

	public void RpcPlaceDarkness(byte playerId, Vector2 position)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Ops.PlaceDarkness);
			writer.Write(playerId);
			writer.Write(position.x);
			writer.Write(position.y);
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

		var reportLock = ButtonLockSystem.CreateOrGetReportButtonLock();
		if (IsPlayerInDarkness(PlayerControl.LocalPlayer))
		{
			reportLock.Lock((int)ButtonLockSystem.ConditionId.Darkpointer);
		}
		else
		{
			reportLock.UnLock((int)ButtonLockSystem.ConditionId.Darkpointer);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart or ResetTiming.ExiledEnd or ResetTiming.OnPlayer)
		{
			ButtonLockSystem.CreateOrGetReportButtonLock().UnLock((int)ButtonLockSystem.ConditionId.Darkpointer);
			lock (this.activeCircles)
			{
				foreach (var circle in this.activeCircles.Values)
				{
					circle.Remove();
				}
				this.activeCircles.Clear();
			}
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.PlaceDarkness:
				byte playerId = msgReader.ReadByte();
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();
				PlaceDarkness(playerId, new Vector2(x, y));
				break;
			case Ops.RemoveDarkness:
				byte removePlayerId = msgReader.ReadByte();
				RemoveDarkness(removePlayerId);
				break;
		}
	}
}
