using ExtremeRoles.Beta;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Module.Interface;

public interface IRaiseHandSystem : IDirtableSystemType
{
	public bool IsInit { get; }

	public static IRaiseHandSystem Get()
	{
		var systemMng = ExtremeSystemTypeManager.Instance;
		if (!systemMng.TryGet<IRaiseHandSystem>(RaiseHandSystem.Type, out var sytem) ||
			sytem == null)
		{
			sytem = PublicBeta.Instance.IsEnableWithMode ? new RaiseHandSystemToggle() : new RaiseHandSystem();
			systemMng.TryAdd(RaiseHandSystem.Type, sytem);
		}
		return sytem;
	}

	public void CreateRaiseHandButton();
	public void AddHand(PlayerVoteArea player);
	public void RaiseHandButtonSetActive(bool active);
}
