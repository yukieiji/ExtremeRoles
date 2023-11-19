using ExtremeRoles.Module.SystemType;
using Hazel;

#nullable enable

namespace ExtremeRoles.Module.Interface;

public interface IDeterioratableExtremeSystemType : IExtremeSystemType
{
	public void Deteriorate(float deltaTime);
}


public interface IExtremeSystemType
{
	public bool IsDirty { get; }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null);

	public void UpdateSystem(PlayerControl player, MessageReader msgReader);

	public void Serialize(MessageWriter writer, bool initialState);

	public void Deserialize(MessageReader reader, bool initialState);
}
