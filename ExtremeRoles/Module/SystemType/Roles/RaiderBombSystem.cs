using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Hazel;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class RaiderBombSystem(RaiderBombSystem.Parameter parameter) : IDirtableSystemType
{
	public enum BombType
	{
		SingleBombType,
		RandomBombType,
		CarpetHorizontalBombType,
		CarpetVerticalBombType
	}

	public sealed record Parameter(
		BombType Type,
		int BombNum,
		float BombRange,
		RaiderBomb.Parameter BombParameter);

	private sealed class Placer(Parameter param)
	{
		private readonly BombType type = param.Type;
		private readonly float range = param.BombRange;
		private readonly float num = param.BombNum;

		private int placedNum = 0;
		private Vector2? target = null;
		private readonly Queue<Vector2> pos = new Queue<Vector2>();

		public void Add(Vector2 pos)
		{
			this.pos.Enqueue(pos);
			updateTargetPos();
		}

		public Vector2? NextPos()
		{
			if (this.target == null)
			{
				return null;
			}

			var offset = this.type switch
			{
				BombType.RandomBombType => Random.insideUnitCircle * Random.Range(0.0f, this.range),
				BombType.CarpetHorizontalBombType => new Vector2((placedNum - Mathf.Floor(this.num / 2.0f)) / this.num * -this.range, 0),
				BombType.CarpetVerticalBombType => new Vector2(0, (placedNum - Mathf.Floor(this.num / 2.0f)) / this.num * -this.range),
				_ => Vector2.zero
			};
			this.placedNum++;
			var result = this.target.Value + offset;
			if (this.placedNum >= this.num)
			{
				this.placedNum = 0;
				this.target = null;
				updateTargetPos();
			}
			return result;
		}

		private void updateTargetPos()
		{
			if (this.target != null)
			{
				return;
			}
			this.target = this.pos.Count > 0 ? this.pos.Dequeue() : null;
		}
	}

	private readonly RaiderBomb.Parameter parameter = parameter.BombParameter;
	private readonly Placer placer = new Placer(parameter);
	private Vector2? target = null;
	private float timer = 0.0f;

	public bool IsDirty { get; private set; }

	public void MarkClean()
	{
		this.IsDirty = false;
	}

	public void Deteriorate(float deltaTime)
	{
		if (this.IsDirty ||
			this.target == null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			return;
		}

		this.timer += deltaTime;
		if (this.timer > 0.5f)
		{
			this.timer = 0.0f;
			this.setBomb(this.target.Value);
			this.IsDirty = true;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		if (this.target.HasValue)
		{
			NetHelpers.WriteVector2(this.target.Value, writer);
		}
		this.target = this.placer.NextPos();
		this.timer = 0.0f;
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		Vector2 pos = NetHelpers.ReadVector2(reader);
		setBomb(pos);
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public static void RpcSetBomb(Vector2 pos)
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			ExtremeSystemType.RaiderBomb, x =>
			{
				NetHelpers.WriteVector2(pos, x);
			});
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var pos = NetHelpers.ReadVector2(msgReader);
		this.placer.Add(pos);

		this.target = this.placer.NextPos();
		if (this.target.HasValue)
		{
			setBomb(this.target.Value);
		}
		this.IsDirty = true;
	}

	private void setBomb(Vector2 pos)
	{
		var bomb = new GameObject("Bomb");
		bomb.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);
		var bombBehe = bomb.AddComponent<RaiderBomb>();
		bombBehe.SetParameter(this.parameter);
	}
}
