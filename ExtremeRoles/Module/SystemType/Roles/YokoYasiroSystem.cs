using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles;
using Hazel;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class YokoYasiroSystem : IDeterioratableExtremeSystemType
{
	public enum Ops : byte
	{
		Set,
		Update,
	}

	public enum YasiroStatus : byte
	{
		Deactivate,
		Activate,
		Sealed,
	}

	private sealed class YasiroInfo
	{
		public int Id { get; init; }
		public Vector2 Pos { get; init; }
		public YasiroStatus CurState { get; private set; }

		public float Timer { get; private set; }

		private bool isActiveTimer = false;

		public YasiroInfo(int id, in Vector2 pos)
		{
			this.Pos = pos;
			this.CurState = YasiroStatus.Deactivate;
			this.Id = id;
			this.isActiveTimer = false;
		}

		public void ResetTimer(in float time)
		{
			this.isActiveTimer = true;
			this.Timer = time;
		}

		public void Update(in float deltaTime)
		{
			if (!this.isActiveTimer) { return; }

			if (this.Timer > 0.0)
			{
				this.Timer -= deltaTime;
				return;
			}
			this.UpdateState();
			this.isActiveTimer = false;
		}

		public void UpdateState()
		{
			this.CurState = (YasiroStatus)(((int)(this.CurState + 1)) % 2);
		}
	}

	private readonly Dictionary<int, YasiroInfo> info = new Dictionary<int, YasiroInfo>();
	private readonly float range;

	private int id = 0;

	public bool IsDirty => false;

	public YokoYasiroSystem(float yasiroRange)
	{
		this.range = yasiroRange;
	}

	public bool IsPlayerProtected(SingleRoleBase role, Vector2 pos)
	{
		if (role.Id != ExtremeRoleId.Yoko)
		{
			return false;
		}

		return this.info.Values.Any(x =>
		{
			if (x.CurState != YasiroStatus.Activate)
			{
				return false;
			}

			Vector2 diff = pos - x.Pos;
			float l2 = diff.magnitude;

			return
				l2 <= this.range &&
				!PhysicsHelpers.AnyNonTriggersBetween(
					pos, diff.normalized,
					l2, Constants.ShipAndObjectsMask);
		});
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Deteriorate(float deltaTime)
	{
		foreach (var info in this.info.Values)
		{
			info.Update(deltaTime);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte fakerOps = msgReader.ReadByte();

		switch ((Ops)fakerOps)
		{
			case Ops.Set:
				float x = msgReader.ReadByte();
				float y = msgReader.ReadByte();
				this.createYasiro(new Vector2(x, y));
				break;
			case Ops.Update:
				int id = msgReader.ReadPackedInt32();
				updateYasiroInfo(id);
				break;
			default:
				return;
		}
	}
	private void createYasiro(in Vector2 pos)
	{
		var newYasiro = new GameObject($"YokoYasiro_{this.id}");
		newYasiro.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

		var newInfo = new YasiroInfo(this.id, newYasiro.transform.position);
		// コンソール追加


		this.id++;
	}

	private void updateYasiroInfo(int id)
	{
		if (!this.info.TryGetValue(id, out var info))
		{
			return;
		}

		info.UpdateState();

		float timer = info.CurState switch
		{
			_ => 0.0f
		};
		if (timer <= 0.0f)
		{
			return;
		}
		info.ResetTimer(timer);
	}
}
