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
		public Vector2? Target { private get; set; } = null;

		public Vector2? NextPos()
		{
			if (this.Target == null)
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
			var result = this.Target.Value + offset;
			if (this.placedNum >= this.num)
			{
				this.placedNum = 0;
				this.Target = null;
			}
			return result;
		}
	}

	private readonly RaiderBomb.Parameter parameter = parameter.BombParameter;
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
