using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class DarkpointerDarknessSystem : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.DarkpointerDarkness;

	public enum Ops : byte
	{
		PlaceDarkness,
	}

	public sealed class DarkCircle
	{
		public int Id { get; }
		public Vector2 Position { get; }
		public float Range { get; }
		public float Timer { get; private set; }

		private GameObject? visualObject;

		public DarkCircle(int id, Vector2 position, float range, float duration)
		{
			this.Id = id;
			this.Position = position;
			this.Range = range;
			this.Timer = duration;

			createVisual();
		}

		public void UpdateTimer(float deltaTime)
		{
			this.Timer -= deltaTime;
		}

		public bool IsExpired => this.Timer <= 0f;

		private void createVisual()
		{
			try
			{
				this.visualObject = new GameObject($"DarkCircle_{this.Id}");
				this.visualObject.transform.position = new Vector3(
					this.Position.x,
					this.Position.y,
					this.Position.y / 1000f - 0.05f);

				var rend = this.visualObject.AddComponent<SpriteRenderer>();
				rend.sprite = GetBlackCircleSprite();
				rend.color = Color.black;
				rend.sortingOrder = 32767;

				float diameter = this.Range * 2f;
				this.visualObject.transform.localScale = new Vector3(diameter, diameter, 1f);
			}
			catch (Exception e)
			{
				Logging.Error($"Failed to create DarkCircle visual: {e}");
			}
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

	private static Sprite? blackCircleSprite;

	public static Sprite GetBlackCircleSprite()
	{
		if (blackCircleSprite == null)
		{
			int size = 128;
			Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
			Color[] colors = new Color[size * size];
			float radius = size / 2f;
			Vector2 center = new Vector2(radius, radius);

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dist = Vector2.Distance(new Vector2(x, y), center);
					if (dist <= radius)
					{
						colors[y * size + x] = Color.black;
					}
					else
					{
						colors[y * size + x] = Color.clear;
					}
				}
			}
			texture.SetPixels(colors);
			texture.Apply();
			blackCircleSprite = Sprite.Create(
				texture,
				new Rect(0, 0, size, size),
				new Vector2(0.5f, 0.5f),
				size);
		}
		return blackCircleSprite;
	}

	private readonly Dictionary<int, DarkCircle> activeCircles = new();
	private int nextId = 0;

	public bool IsDirty { get; private set; }

	public void MarkClean()
	{
		this.IsDirty = false;
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

	public void PlaceDarkness(Vector2 position, float range, float duration)
	{
		lock (this.activeCircles)
		{
			int circleId = this.nextId++;
			var circle = new DarkCircle(circleId, position, range, duration);
			this.activeCircles[circleId] = circle;
		}
		this.IsDirty = true;
	}

	public void RpcPlaceDarkness(Vector2 position, float range, float duration)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(Type, writer =>
		{
			writer.Write((byte)Ops.PlaceDarkness);
			writer.Write(position.x);
			writer.Write(position.y);
			writer.Write(range);
			writer.Write(duration);
		});
	}

	public void Deteriorate(float deltaTime)
	{
		if (MeetingHud.Instance != null || ExileController.Instance != null)
		{
			return;
		}

		List<int> expired = new();
		lock (this.activeCircles)
		{
			foreach (var (id, circle) in this.activeCircles)
			{
				circle.UpdateTimer(deltaTime);
				if (circle.IsExpired)
				{
					expired.Add(id);
				}
			}

			foreach (int id in expired)
			{
				if (this.activeCircles.TryGetValue(id, out var circle))
				{
					circle.Remove();
					this.activeCircles.Remove(id);
					this.IsDirty = true;
				}
			}
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
				float x = msgReader.ReadSingle();
				float y = msgReader.ReadSingle();
				float range = msgReader.ReadSingle();
				float duration = msgReader.ReadSingle();
				PlaceDarkness(new Vector2(x, y), range, duration);
				break;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		lock (this.activeCircles)
		{
			writer.WritePacked(this.activeCircles.Count);
			foreach (var circle in this.activeCircles.Values)
			{
				writer.WritePacked(circle.Id);
				writer.Write(circle.Position.x);
				writer.Write(circle.Position.y);
				writer.Write(circle.Range);
				writer.Write(circle.Timer);
			}
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		lock (this.activeCircles)
		{
			foreach (var circle in this.activeCircles.Values)
			{
				circle.Remove();
			}
			this.activeCircles.Clear();

			int count = reader.ReadPackedInt32();
			for (int i = 0; i < count; i++)
			{
				int id = reader.ReadPackedInt32();
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				float range = reader.ReadSingle();
				float timer = reader.ReadSingle();

				var circle = new DarkCircle(id, new Vector2(x, y), range, timer);
				this.activeCircles[id] = circle;
				if (id >= this.nextId)
				{
					this.nextId = id + 1;
				}
			}
		}
	}
}
