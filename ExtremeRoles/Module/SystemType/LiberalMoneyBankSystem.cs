using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using Hazel;

namespace ExtremeRoles.Module.SystemType;

public class LiberalMoneyBankSystem : IDirtableSystemType
{
    public const ExtremeSystemType SystemType = ExtremeSystemType.LiberalMoneyBank;

    public bool IsDirty { get; private set; }
    public float Money { get; private set; }
	public float WinMoney { get; init; } = 0.5f;
	public bool IsLeaderDead { get; private set; }

	public static bool IsCanKillTo(byte targetPlayerId)
		=> ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) &&
			targetRole.Core.Id is ExtremeRoleId.Leader &&
			ExtremeSystemTypeManager.Instance.TryGet<LiberalMoneyBankSystem>(ExtremeSystemType.LiberalMoneyBank, out var system) &&
			system.IsLeaderDead;

	public void MarkClean()
    {
        IsDirty = false;
    }

    public void Deteriorate(float deltaTime)
    {
        // TODO: Implement money gain logic here.
        // For now, just increment money for testing purposes.
        if (Money >= this.WinMoney)
        {
            IsDirty = true;
        }
    }

	public void RpcAddKillMoney()
	{

	}

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write(Money);
    }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        Money = reader.ReadSingle();
    }

    public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
    {
        if (timing == ResetTiming.MeetingStart)
        {
            
        }
        Money = 0f;
        IsDirty = false;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        // This system is host-authoritative, so we don't need to do anything here.
    }
}
