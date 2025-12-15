using Hazel;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.GameResult;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public class LiberalMoneyBankSystem(LiberalDefaultOptionLoader option) : IDirtableSystemType
{
    public const ExtremeSystemType SystemType = ExtremeSystemType.LiberalMoneyBank;

    public bool IsDirty { get; private set; }
	public float Money { get; private set; } = 0.0f;
	
	public float WinMoney { get; init; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney);

	private float boost = 1.0f;

	public readonly DeltaInfo delta = new DeltaInfo();

	public class DeltaInfo
	{
		public byte PlayerId { get; private set; }
		public LiberalMoneyHistory.Reason Reason { get; private set; }

		public float Money { get; private set; } = 0.0f;
		public float Boost { get; private set; } = 0.0f;
		
		public bool IsDirty => this.Money > 0.0f || this.Boost > 0.0f;

		public void Clear()
		{
			this.Money = 0.0f;
			this.Boost = 0.0f;
		}

		public void Serialize(MessageWriter writer)
		{
			writer.Write(this.Money);
			writer.Write(this.Boost);
		}

		public void Deserialize(MessageReader reader)
		{
			this.PlayerId = reader.ReadByte();
			this.Reason = (LiberalMoneyHistory.Reason)reader.ReadByte();
			this.Money = reader.ReadSingle();
			this.Boost = reader.ReadSingle();
		}
	}

	public void MarkClean()
    {
		this.delta.Clear();
        IsDirty = false;
    }

    public void Deteriorate(float deltaTime)
    {

    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        if (this.delta.IsDirty)
		{
			this.delta.Serialize(writer);
			this.delta.Clear();
		}
		this.IsDirty = initialState;
	}

    public void Deserialize(MessageReader reader, bool initialState)
    {
		desirialize(reader);
		this.delta.Clear();
    }

    public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
    {

    }

	public static void RpcUpdateSystem(
		byte playerId,
		LiberalMoneyHistory.Reason reason,
		float deltaMoney = 0.0f,
		float deltaBoost = 0.0f)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			SystemType,
			x =>
			{
				x.Write(playerId);
				x.Write((byte)reason);
				x.Write(deltaMoney);
				x.Write(deltaBoost);
			});
	}

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
		// ホストのみ
		desirialize(msgReader);

		Helper.Logging.Debug($"まねー: {this.Money}");
		Helper.Logging.Debug($"ブースト: {this.boost}");

		this.IsDirty = this.delta.IsDirty;
	}

	private void desirialize(MessageReader msgReader)
	{
		this.delta.Deserialize(msgReader);

		this.boost += this.delta.Boost;
		float amount = this.boost * this.delta.Money;
		this.Money += amount;
		LiberalMoneyHistory.Add(new LiberalMoneyHistory.MoneyHistory(this.delta.Reason, this.delta.PlayerId, amount));
	}
}
