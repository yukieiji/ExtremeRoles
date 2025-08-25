using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Performance;
using System.Linq;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Extension.Linq;
using System;
using System.Buffers;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class GlitchDummySystem(
	bool isBlockImp,
	bool isBlockMarlin,
	float activeTime) : IDirtableSystemType
{
	public bool IsDirty => false;

	private readonly bool isBlockImp = isBlockImp;
	private readonly bool isBlockMarlin = isBlockMarlin;

	private abstract class GlitchBase(float time)
	{
		private float time;
		private readonly float activeTime = time;
		private TargetInfo? target;

		public void Update(float deltaTime)
		{
			if (this.time < 0.0f)
			{
				return;
			}

			this.time -= deltaTime;

			if (this.time < 0.0f)
			{
				this.DeActive();
			}
		}

		public void Reset()
		{
			this.time = -1.0f;
			this.DeActive();
		}
		public void Active(TargetInfo target)
		{
			this.time = this.activeTime;
			this.target = target;
			this.ActiveImp(this.target);
		}

		public void DeActive()
		{
			if (this.target is null)
			{
				return;
			}
			this.DeActiveImp(this.target);
			this.target = null;
		}
		public void SetUp()
		{
			if (this.time < 0.0f || this.target is null)
			{
				return;
			}
			SetUpImp(this.target);
		}
		protected abstract void SetUpImp(TargetInfo target);
		protected abstract void DeActiveImp(TargetInfo target);
		protected abstract void ActiveImp(TargetInfo target);
	}

	private sealed class AdminGlitch(float time) : GlitchBase(time)
	{
		private readonly AdminDummySystem admin = AdminDummySystem.Get();
		private readonly List<(SystemTypes, int)> dummy = new List<(SystemTypes, int)>();
		private AdminDummySystem.DummyMode prevMode;

		protected override void SetUpImp(TargetInfo target)
		{
			var room = Player.TryGetPlayerRoom(target.LocalPlayer, out var systemType) ? systemType.Value : SystemTypes.Hallway;
			int color = target.LocalPlayer.CurrentOutfit.ColorId;
			this.admin.Add(room, color);
		}

		protected override void DeActiveImp(TargetInfo target)
		{
			this.admin.Mode = this.prevMode;
			foreach (var (room, color) in this.dummy)
			{
				this.admin.Remove(room, color);
			}
			this.dummy.Clear();
		}

		protected override void ActiveImp(TargetInfo target)
		{
			this.prevMode = this.admin.Mode;
			this.admin.Mode = AdminDummySystem.DummyMode.Override;
			var colors = target.Target.Select(x => x.CurrentOutfit.ColorId);
			var allRoom = ShipStatusCache.KeyedRoom.Keys.ToArray();
			int maxIndex = allRoom.Length;

			foreach (int color in colors)
			{
				int targetIndex = RandomGenerator.Instance.Next(maxIndex);
				var room = allRoom[targetIndex];
				this.admin.Add(room, color);
				this.dummy.Add((room, color));
			}
		}
	}

	private sealed class SecurityGlitch(float time) : GlitchBase(time)
	{
		private readonly SecurityDummySystemManager security = SecurityDummySystemManager.Get();
		private SecurityDummySystemManager.DummyMode prevMode;
		private byte[] dummy = [];

		protected override void SetUpImp(TargetInfo target)
		{
			this.security.Remove(target.LocalPlayer.PlayerId);
		}

		protected override void DeActiveImp(TargetInfo target)
		{
			this.security.Mode = this.prevMode;
			if (this.security.IsLog)
			{
				this.security.Remove(target.Target.Select(x => x.PlayerId).ToArray());
				if (this.dummy.Length > 0)
				{
					this.security.Remove(this.dummy);
					this.dummy = [];
				}
			}
			else
			{
				this.security.Remove(
					PlayerCache.AllPlayerControl.Select(x => x.PlayerId).ToArray());
			}
			this.security.IsActive = false;
		}

		protected override void ActiveImp(TargetInfo target)
		{
			this.prevMode = this.security.Mode;
			this.security.Mode = SecurityDummySystemManager.DummyMode.Normal;
			if (this.security.IsLog)
			{
				this.security.Add(target.Target.Select(x => x.PlayerId).ToArray());
				int size = target.Target.Count;
				if (size < 5)
				{
					int addNum = RandomGenerator.Instance.Next(5, 10) - size + 1;
					this.dummy = target.Alive.GetRandomItem(addNum).Select(x => x.PlayerId).ToArray();
					this.security.Add(this.dummy);
				}
			}
			else
			{
				this.security.Add(
					PlayerCache.AllPlayerControl.Select(x => x.PlayerId).ToArray());
			}
			this.security.IsActive = true;
		}
	}

	private sealed class VitalGlitch(float time) : GlitchBase(time)
	{
		private readonly VitalDummySystem vital = VitalDummySystem.Get();
		private VitalDummySystem.DummyMode prevMode;

		private readonly HashSet<byte> dummyDead = new HashSet<byte>();

		protected override void SetUpImp(TargetInfo _)
		{
			if (PlayerControl.LocalPlayer == null ||
				PlayerControl.LocalPlayer.Data == null)
			{
				return;
			}

			byte playerId = PlayerControl.LocalPlayer.PlayerId;
			var data = PlayerControl.LocalPlayer.Data;

			if (data.IsDead)
			{
				this.vital.AddDead(playerId);
			}
			else if (data.Disconnected)
			{
				 this.vital.AddDisconnect(playerId);
			}
			else
			{
				this.vital.AddAlive(playerId);
			}
		}

		protected override void DeActiveImp(TargetInfo _)
		{
			foreach (byte dead in this.dummyDead)
			{
				this.vital.RemoveDead(dead);
			}
			this.dummyDead.Clear();
			this.vital.Mode = this.prevMode;
			this.vital.IsActive = false;
		}

		protected override void ActiveImp(TargetInfo target)
		{
			this.vital.IsActive = true;
			this.prevMode = this.vital.Mode;
			this.vital.Mode = VitalDummySystem.DummyMode.OnceRandom;
			int num = RandomGenerator.Instance.Next(1, 3);
			foreach (var player in target.Target)
			{
				num--;
				byte playerId = player.PlayerId;
				this.vital.AddDead(playerId);
				this.dummyDead.Add(playerId);
				if (num <= 0)
				{
					return;
				}
			}

			var buff = new HashSet<int>(num);

			while (num != 0)
			{
				int index = 0;
				do
				{
					// 同じ分だったとき
					if (buff.Count == target.Alive.Count)
					{
						return;
					}
					index = RandomGenerator.Instance.Next(0, target.Alive.Count);

				} while (buff.Contains(index));

				buff.Add(index);

				byte targetPlayer = target.Alive[index].PlayerId;
				if (targetPlayer == target.LocalPlayer.PlayerId)
				{
					continue;
				}

				num--;

				this.vital.AddDead(targetPlayer);
				this.dummyDead.Add(targetPlayer);
			}
		}
	}

	private sealed class TargetInfo
	{
		public enum LocalPlayerInfo
		{
			Alive,
			Dead,
			Disconected
		}

		public LocalPlayerInfo LocalStatus { get; }

		public IReadOnlyList<PlayerControl> Alive => alive;
		public IReadOnlyList<PlayerControl> Dead => dead;
		public IReadOnlyList<PlayerControl> Disconected => disconected;
		public IReadOnlyList<PlayerControl> Target => target;
		public PlayerControl LocalPlayer { get; } = PlayerControl.LocalPlayer;

		private readonly List<PlayerControl> alive = [];
		private readonly List<PlayerControl> dead = [];
		private readonly List<PlayerControl> disconected = [];
		private readonly List<PlayerControl> target = [];

		public TargetInfo()
		{

			foreach (var player in PlayerCache.AllPlayerControl)
			{
				if (player == null ||
					player.Data == null)
				{
					continue;
				}

				bool isLocal = PlayerControl.LocalPlayer.PlayerId == player.PlayerId;

				if (player.Data.IsDead)
				{
					this.dead.Add(player);
					if (isLocal)
					{
						this.LocalStatus = LocalPlayerInfo.Dead;
					}
				}
				else if (player.Data.Disconnected)
				{
					this.disconected.Add(player);
					if (isLocal)
					{
						this.LocalStatus = LocalPlayerInfo.Disconected;
					}
				}
				else
				{
					this.alive.Add(player);
					if (isLocal)
					{
						this.LocalStatus = LocalPlayerInfo.Alive;
					}
					int index = RandomGenerator.Instance.Next(2);
					if (index == 1)
					{
						this.target.Add(player);
					}
				}
			}
		}
	}


	private static TargetInfo target => new TargetInfo();

	private readonly AdminGlitch admin = new AdminGlitch(activeTime);
	private readonly SecurityGlitch security = new SecurityGlitch(activeTime);
	private readonly VitalGlitch vital = new VitalGlitch(activeTime);


	public void MarkClean()
	{
	}

	public void Deteriorate(float deltaTime)
	{
		this.admin.Update(deltaTime);
		this.vital.Update(deltaTime);
		this.security.Update(deltaTime);
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart)
		{
			this.admin.Reset();
			this.vital.Reset();
			this.security.Reset();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var targetDevice = (SystemConsoleType)msgReader.ReadByte();
		var role = ExtremeRoleManager.GetLocalPlayerRole();

		if (this.isBlockImp &&
			(
				role.IsImpostor() ||
				(this.isBlockMarlin && role.Core.Id == ExtremeRoleId.Marlin)
			))
		{
			return;
		}

		switch (targetDevice)
		{
			case SystemConsoleType.AdminModule:
				this.admin.Active(target);
				break;
			case SystemConsoleType.SecurityCamera:
				this.security.Active(target);
				break;
			case SystemConsoleType.VitalsLabel:
				this.vital.Active(target);
				break;
			default:
				break;
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{ }
	public void Serialize(MessageWriter writer, bool initialState)
	{ }

}
