using Hazel;

using ExtremeRoles.Module.Interface;

using ExtremeRoles.Patches.LogicGame;

namespace ExtremeRoles.Module.SystemType;

public sealed class MeetingTimeChangeSystem : IExtremeSystemType
{
	public record struct MeetingHudTimerOffset(
		int Discussion = 0,
		int Voting = 0)
	{
		public void Update(int newOffset)
		{
			this.Voting = 0;
			this.Discussion = 0;

			int discussionTime = MeetingHudTimerOffsetPatch.NoModDiscussionTime;
			// オフセット値が+もしくは、オフセット値が負でも議論時間が残る場合は議論時間だけ変更する
			if (newOffset > 0.0f || (discussionTime > 0.0f && discussionTime + newOffset >= 0.0f))
			{
				this.Discussion = newOffset;
				this.Voting = 0;
			}
			else
			{
				this.Discussion = discussionTime;
				this.Voting = newOffset + discussionTime;
			}
		}

		public void Reset()
		{
			this.Discussion = 0;
			this.Voting = 0;
		}
	}

	public enum Ops : byte
	{
		ChangeMeetingHudPermOffset,
		ChangeMeetingHudTempOffset,
		ChangeButtonTime,
		Reset,
	}

	public MeetingHudTimerOffset HudTimerOffset { get; }

	public int PermOffset { get; private set; } = 0;
	public int TempOffset { get; private set; } = 0;

	public int ButtonTimeOffset { get; private set; } = 0;

	public bool IsDirty { get; set; }

	public MeetingTimeChangeSystem()
	{
		this.HudTimerOffset = new MeetingHudTimerOffset();
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.PermOffset = reader.ReadPackedInt32();
		this.TempOffset = reader.ReadPackedInt32();
		this.ButtonTimeOffset = reader.ReadPackedInt32();

		this.updateMeetingOffset();
	}

	public void Detoriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingEnd &&
			AmongUsClient.Instance.AmHost)
		{
			this.TempOffset = 0;
			this.IsDirty = true;
			this.updateMeetingOffset();
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
				updateMeetingOffset();
				break;
			case Ops.ChangeMeetingHudTempOffset:
				this.TempOffset = msgReader.ReadPackedInt32();
				updateMeetingOffset();
				break;
			case Ops.ChangeButtonTime:
				this.ButtonTimeOffset = msgReader.ReadPackedInt32();
				break;
			case Ops.Reset:
				this.ButtonTimeOffset = 0;
				this.TempOffset = 0;
				this.PermOffset = 0;
				this.updateMeetingOffset();
				break;
			default:
				return;
		}
		this.IsDirty = true;
	}

	private void updateMeetingOffset()
	{
		this.HudTimerOffset.Update(this.TempOffset + this.PermOffset);
	}
}
