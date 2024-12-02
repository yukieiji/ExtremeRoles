using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.SecurityDummySystem;
using ExtremeRoles.Performance;
using System.Linq;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class GlitchDummySystem(
	bool isBlockImp,
	float activeTime) : IDirtableSystemType
{
	public bool IsDirty => false;

	private readonly bool isBlockImp = isBlockImp;

	private abstract class GlitchBase(float time)
	{
		private float time;
		private readonly float activeTime = time;

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
			this.ActiveImp(target);
		}
		public abstract void SetUp();
		public abstract void DeActive();
		protected abstract void ActiveImp(TargetInfo target);
	}

	private sealed class AdminGlitch(float time) : GlitchBase(time)
	{
		private readonly AdminDummySystem admin = AdminDummySystem.Get();
		private readonly List<(SystemTypes, int)> dummy = new List<(SystemTypes, int)>();
		private AdminDummySystem.DummyMode prevMode;
		private TargetInfo? target;

		public override void SetUp()
		{
			if (this.target is null)
			{
				return;
			}

			var room = Player.TryGetPlayerRoom(this.target.LocalPlayer, out var systemType) ? systemType.Value : SystemTypes.Hallway;
			int color = this.target.LocalPlayer.CurrentOutfit.ColorId;
			this.admin.Add(room, color);
		}

		public override void DeActive()
		{
			if (this.target is null)
			{
				return;
			}

			this.admin.Mode = this.prevMode;
			foreach (var (room, color) in this.dummy)
			{
				this.admin.Remove(room, color);
			}
			this.dummy.Clear();
			this.target = null;
		}

		protected override void ActiveImp(TargetInfo target)
		{
			this.prevMode = this.admin.Mode;
			this.admin.Mode = AdminDummySystem.DummyMode.Override;
			var colors = target.Target.Select(x => x.CurrentOutfit.ColorId);
			var allRoom = CachedShipStatus.FastRoom.Keys.ToArray();
			int maxIndex = allRoom.Length;

			foreach (int color in colors)
			{
				int targetIndex = RandomGenerator.Instance.Next(maxIndex);
				var room = allRoom[targetIndex];
				this.admin.Add(room, color);
				this.dummy.Add((room, color));
			}
			this.target = target;
		}
	}

	private sealed class SecurityGlitch(float time) : GlitchBase(time)
	{
		private readonly SecurityDummySystemManager security = SecurityDummySystemManager.Get();
		private SecurityDummySystemManager.DummyMode prevMode;
		private TargetInfo? target;

		public override void SetUp()
		{
			if (this.target is null)
			{
				return;
			}
			this.security.Remove(target.LocalPlayer.PlayerId);
		}

		public override void DeActive()
		{
			if (this.target is null)
			{
				return;
			}
			this.security.Mode = this.prevMode;
			if (this.security.IsLog)
			{
				this.security.Remove(this.target.Target.Select(x => x.PlayerId).ToArray());
			}
			else
			{
				this.security.Remove(this.target.Target.Select(x => x.PlayerId).ToArray());
				this.security.Remove(this.target.Dead.Select(x => x.PlayerId).ToArray());
			}
			this.target = null;
			this.security.IsActive = false;
		}

		protected override void ActiveImp(TargetInfo target)
		{
			this.prevMode = this.security.Mode;
			this.security.Mode = SecurityDummySystemManager.DummyMode.Normal;
			if (this.security.IsLog)
			{
				this.security.Add(target.Target.Select(x => x.PlayerId).ToArray());
			}
			else
			{
				this.security.Add(target.Target.Select(x => x.PlayerId).ToArray());
				this.security.Add(target.Dead.Select(x => x.PlayerId).ToArray());
			}
			this.target = target;
			this.security.IsActive = true;
		}
	}

	private sealed class VitalGlitch(float time) : GlitchBase(time)
	{
		private readonly VitalDummySystem vital = VitalDummySystem.Get();
		private VitalDummySystem.DummyMode prevMode;
		private TargetInfo? target;

		private readonly HashSet<byte> dummyDead = new HashSet<byte>();
		private readonly HashSet<byte> dummyDisconnect = new HashSet<byte>();

		public override void SetUp()
		{
			if (this.target is null ||
				PlayerControl.LocalPlayer == null ||
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

		public override void DeActive()
		{
			if (this.target is null)
			{
				return;
			}
			foreach (byte dead in this.dummyDead)
			{
				this.vital.RemoveDead(dead);
			}
			foreach (byte disconnect in this.dummyDisconnect)
			{
				this.vital.RemoveDisconnect(disconnect);
			}
			this.dummyDisconnect.Clear();
			this.dummyDead.Clear();
			this.vital.Mode = this.prevMode;
		}

		protected override void ActiveImp(TargetInfo target)
		{
			this.prevMode = this.vital.Mode;
			this.vital.Mode = VitalDummySystem.DummyMode.OnceRandom;
			foreach (var player in target.Target)
			{
				int index = RandomGenerator.Instance.Next(3);
				byte playerId = player.PlayerId;
				switch (index)
				{
					case 1:
						this.vital.AddDead(playerId);
						this.dummyDead.Add(playerId);
						break;
					case 2:
						this.vital.AddDisconnect(playerId);
						this.dummyDisconnect.Add(playerId);
						break;
					default:
						break;
				}


			}
			this.target = target;
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


	private float time;

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

		if (this.isBlockImp && (role.IsImpostor() || role.Id == ExtremeRoleId.Marlin))
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
