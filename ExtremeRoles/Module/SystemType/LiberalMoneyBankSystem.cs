using ExtremeRoles.Module.Interface;
using Hazel;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles.Module.SystemType;

public class LiberalMoneyBankSystem : IDirtableSystemType
{
    public const ExtremeSystemType SystemType = ExtremeSystemType.LiberalMoneyBank;

    public bool IsDirty { get; private set; }
    public float Money { get; private set; }
    private float winMoney;

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Deteriorate(float deltaTime)
    {
        // TODO: Implement money gain logic here.
        // For now, just increment money for testing purposes.
        Money += deltaTime;
        if (Money >= winMoney)
        {
            IsDirty = true;
        }
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
