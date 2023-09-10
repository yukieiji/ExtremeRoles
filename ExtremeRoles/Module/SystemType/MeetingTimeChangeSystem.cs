using Hazel;

using ExtremeRoles.Module.Interface;

using ExtremeRoles.Patches.LogicGame;

namespace ExtremeRoles.Module.SystemType;

public sealed class MeetingTimeChangeSystem : IExtremeSystemType
{
	public record struct MeetingHudTimerOffset(
		float Discussion = 0.0f,
		float Voting = 0.0f)
	{
		public void Update(float newOffset)
		{
			this.Voting = 0.0f;
			this.Discussion = 0.0f;

			float discussionTime = MeetingHudTimerOffsetPatch.NoModDiscussionTime;
			// オフセット値が+もしくは、オフセット値が負でも議論時間が残る場合は議論時間だけ変更する
			if (newOffset > 0.0f || (discussionTime > 0.0f && discussionTime + newOffset >= 0.0f))
			{
				this.Discussion = newOffset;
				this.Voting = 0.0f;
			}
			else
			{
				this.Discussion = discussionTime;
				this.Voting = newOffset + discussionTime;
			}
		}

		public void Reset()
		{
			this.Discussion = 0.0f;
			this.Voting = 0.0f;
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

	public float PermOffset { get; private set; } = 0.0f;
	public float TempOffset { get; private set; } = 0.0f;

	public float ButtonTimeOffset { get; private set; } = 0.0f;

	public bool IsDirty { get; set; }

	public MeetingTimeChangeSystem()
	{
		this.HudTimerOffset = new MeetingHudTimerOffset();
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.PermOffset = reader.ReadSingle();
		this.TempOffset = reader.ReadSingle();
		this.ButtonTimeOffset = reader.ReadSingle();

		this.updateMeetingOffset();
	}

	public void Detoriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.PermOffset);
		writer.Write(this.TempOffset);
		writer.Write(this.ButtonTimeOffset);
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.ChangeMeetingHudPermOffset:
				this.PermOffset = msgReader.ReadSingle();
				updateMeetingOffset();
				break;
			case Ops.ChangeMeetingHudTempOffset:
				this.TempOffset = msgReader.ReadSingle();
				updateMeetingOffset();
				break;
			case Ops.ChangeButtonTime:
				this.ButtonTimeOffset = msgReader.ReadSingle();
				break;
			case Ops.Reset:
				this.ButtonTimeOffset = 0.0f;
				this.TempOffset = 0.0f;
				this.PermOffset = 0.0f;
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
