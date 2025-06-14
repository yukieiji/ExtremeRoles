using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;

public sealed class SurrogatorStatus(
	bool hasTask,
	int yandereSeeTaskRate) : IStatusModel
{
	private readonly bool hasTask = hasTask;
	private readonly float yandereSeeTaskRate = yandereSeeTaskRate / 100.0f;

	public bool SeeYandere { get; private set; } = false;

	public void Update(PlayerControl surrogatorPlayer)
	{
		if (!this.hasTask || this.SeeYandere)
		{
			return;
		}

		float taskGage = Helper.Player.GetPlayerTaskGage(surrogatorPlayer);
		if (taskGage >= this.yandereSeeTaskRate)
		{
			this.SeeYandere = true;
		}
	}
}
