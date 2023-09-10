using Hazel;

using ExtremeRoles.Module.Interface;

using ExtremeRoles.Patches.LogicGame;

namespace ExtremeRoles.Module.SystemType;

public sealed class MeetingTimeOffsetSystem : IExtremeSystemType
{
	public enum Ops : byte
	{
		ChangeMeetingHudOffset,
		ChangeButtonTime,
		Reset,
	}

	public float MeetingHudTimerOffset => this.DiscussionTimeOffset + this.VotingTimeOffset;

	public float DiscussionTimeOffset { get; private set; } = 0.0f;
	public float VotingTimeOffset { get; private set; } = 0.0f;

	public float ButtonTimeOffset { get; private set; } = 0.0f;

	public bool IsDirty { get; set; }

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.DiscussionTimeOffset = reader.ReadSingle();
		this.VotingTimeOffset = reader.ReadSingle();
		this.ButtonTimeOffset = reader.ReadSingle();
	}

	public void Detoriorate(float deltaTime)
	{ }

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.DiscussionTimeOffset);
		writer.Write(this.VotingTimeOffset);
		writer.Write(this.ButtonTimeOffset);
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.ChangeMeetingHudOffset:

				float newOffset = msgReader.ReadSingle();
				float discussionTime = MeetingHudTimerOffsetPatch.NoModDiscussionTime;
				// オフセット値が+もしくは、オフセット値が負でも議論時間が残る場合は議論時間だけ変更する
				if (newOffset > 0.0f || (discussionTime > 0.0f && discussionTime + newOffset >= 0.0f))
				{
					this.DiscussionTimeOffset = newOffset;
					this.VotingTimeOffset = 0.0f;
					break;
				}
				else
				{
					this.DiscussionTimeOffset = discussionTime;
					this.VotingTimeOffset = newOffset + discussionTime;
				}
				break;
			case Ops.ChangeButtonTime:
				this.ButtonTimeOffset = msgReader.ReadSingle();
				break;
			case Ops.Reset:
				this.ButtonTimeOffset = 0.0f;
				this.DiscussionTimeOffset = 0.0f;
				this.VotingTimeOffset = 0.0f;
				break;
			default:
				return;
		}
		this.IsDirty = true;
	}
}
