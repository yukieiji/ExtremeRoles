using Hazel;

using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.SystemType;

public sealed class MeetingTimeChangeSystem : IExtremeSystemType
{
	public enum Ops : byte
	{
		ChangeMeetingHudPermOffset,
		ChangeMeetingHudTempOffset,
		ChangeButtonTime,
		Reset,
	}

	public int PermOffset { get; private set; } = 0;
	public int TempOffset { get; private set; } = 0;

	public int ButtonTimeOffset { get; private set; } = 0;

	public bool IsDirty { get; set; } = false;

	public int HudTimerStartOffset => this.PermOffset + this.TempOffset;

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.PermOffset = reader.ReadPackedInt32();
		this.TempOffset = reader.ReadPackedInt32();
		this.ButtonTimeOffset = reader.ReadPackedInt32();
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingEnd &&
			AmongUsClient.Instance.AmHost &&
			!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
		{
			this.TempOffset = 0;
			this.IsDirty = true;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.PermOffset);
		writer.WritePacked(this.TempOffset);
		writer.WritePacked(this.ButtonTimeOffset);
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.ChangeMeetingHudPermOffset:
				this.PermOffset = msgReader.ReadPackedInt32();
				break;
			case Ops.ChangeMeetingHudTempOffset:
				this.TempOffset = msgReader.ReadPackedInt32();
				break;
			case Ops.ChangeButtonTime:
				this.ButtonTimeOffset = msgReader.ReadPackedInt32();
				break;
			case Ops.Reset:
				this.ButtonTimeOffset = 0;
				this.TempOffset = 0;
				this.PermOffset = 0;
				break;
			default:
				return;
		}
		this.IsDirty = true;
	}
}
