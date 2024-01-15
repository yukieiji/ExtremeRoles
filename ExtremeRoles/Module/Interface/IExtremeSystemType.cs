using ExtremeRoles.Module.SystemType;
using Hazel;

#nullable enable

namespace ExtremeRoles.Module.Interface;

public interface ISabotageExtremeSystemType : IDirtableSystemType
{
	public bool IsBlockOtherSabotage => this.IsActive;
	public bool IsActive { get; }

	public void Clear();
}

public interface IDirtableSystemType : IExtremeSystemType
{
	public bool IsDirty { get; }

	public void Deteriorate(float deltaTime)
	{ }

	public void Serialize(MessageWriter writer, bool initialState);

	public void Deserialize(MessageReader reader, bool initialState);
}

public interface IExtremeSystemType
{
	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null);

	public void UpdateSystem(PlayerControl player, MessageReader msgReader);
}
