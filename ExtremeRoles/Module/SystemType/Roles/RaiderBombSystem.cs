using Hazel;

using UnityEngine;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class RaiderBombSystem(RaiderBombSystem.SystemParameter parameter) : IDirtableSystemType
{
	public enum BombType
	{
		SingleBomb,
		RandomBomb,
		CarpetHorizontalBomb,
		CarpetVerticalBomb
	}

	public sealed record SystemParameter(
		BombType Type,
		int BombNum,
		float BombRange,
		BombParameter BombParameter);

	public sealed record BombParameter(float Range, float Time);

	private sealed class Placer(SystemParameter param)
	{
		private readonly BombType type = param.Type;
		private readonly float range = param.BombRange;
		private readonly float num = param.BombNum;

		private int placedNum = 0;
		public Vector2? Target { private get; set; } = null;

		public Vector2? NextPos()
		{
			if (this.Target == null)
			{
				return null;
			}

			var offset = this.type switch
			{
				BombType.RandomBomb => Random.insideUnitCircle * this.range,
				BombType.CarpetHorizontalBomb => new Vector2((placedNum - Mathf.Floor(placedNum / 2.0f)) * this.range, 0),
				BombType.CarpetVerticalBomb => new Vector2(0, (placedNum - Mathf.Floor(placedNum / 2.0f)) * this.range),
				_ => Vector2.zero
			};
			this.placedNum++;
			var result = this.Target.Value + offset;
			if (this.placedNum >= this.num)
			{
				this.Target = null;
			}
			return result;
		}
	}

	private readonly BombParameter parameter = parameter.BombParameter;
	private readonly Placer placer = new Placer(parameter);
	private Vector2? target = null;
	private float timer = 0.0f;

	public bool IsDirty { get; private set; }

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
		this.placer.Target = NetHelpers.ReadVector2(msgReader);
		this.IsDirty = true;
	}

	private void setBomb(Vector2 pos)
	{
		var bomb = new GameObject("Bomb");
		bomb.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

	}
}
