using Hazel;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public class LiberalMoneyBankSystem(LiberalDefaultOptipnLoader option) : IDirtableSystemType
{
    public const ExtremeSystemType SystemType = ExtremeSystemType.LiberalMoneyBank;

    public bool IsDirty { get; private set; }
	public float Money { get; private set; } = 0.0f;
	
	public float WinMoney { get; init; } = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney);

	private float boost = 1.0f;

	public readonly DeltaInfo delta = new DeltaInfo();

	public class DeltaInfo
	{
		public float Money { get; set; } = 0.0f;
		public float Boost { get; set; } = 0.0f;
		
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
		this.delta.Deserialize(reader);
		
		this.boost += this.delta.Boost;
		this.Money += (this.delta.Money * this.boost);

		this.delta.Clear();
    }

    public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
    {

    }

	public static void RpcUpdateSystem(float deltaMoney=0.0f, float deltaBoost = 0.0f)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			SystemType,
			x =>
			{
				x.Write(deltaMoney);
				x.Write(deltaBoost);
			});
	}

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
		// ホストのみ
		this.delta.Money += msgReader.ReadSingle();
		this.delta.Boost += msgReader.ReadSingle();

		this.Money += this.delta.Money;
		this.boost += this.delta.Boost;

		Helper.Logging.Debug($"まねー: {this.Money}");
		Helper.Logging.Debug($"ブースト: {this.boost}");

		this.IsDirty = this.delta.IsDirty;
	}
}
