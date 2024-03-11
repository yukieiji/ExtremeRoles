using ExtremeRoles.Beta;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Module.Interface;

public interface IRaiseHandSystem : IDirtableSystemType
{
	public bool IsInit { get; }

	public static IRaiseHandSystem Get()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<IRaiseHandSystem>(
			RaiseHandSystem.Type,
			() => PublicBeta.Instance.IsEnableWithMode ?
				new RaiseHandSystemToggle() : new RaiseHandSystem());

	public void CreateRaiseHandButton();
	public void AddHand(PlayerVoteArea player);
	public void RaiseHandButtonSetActive(bool active);
}
