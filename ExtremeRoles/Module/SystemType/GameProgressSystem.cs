using ExtremeRoles.Module.Interface;

using Hazel;

namespace ExtremeRoles.Module.SystemType;

public sealed class GameProgressSystem : IExtremeSystemType
{
	public enum Progress
	{
		None = 0,
		IntroStart,
		RoleSetUpStart,
		RoleSetUpEnd,
		IntroEnd,
		PreTask,
		Task,
		Meeting,
	}

	public static Progress Cur
	{
		get => get();
		set => set(value);
	}

	private Progress cur = Progress.None;

	private static Progress get()
		=> ExtremeSystemTypeManager.Instance.TryGet<GameProgressSystem>(ExtremeSystemType.GameProgress, out var system) ? system.cur : Progress.None;

	private static void set(Progress newProgress)
	{
		if (ExtremeSystemTypeManager.Instance.TryGet<GameProgressSystem>(ExtremeSystemType.GameProgress, out var system))
		{
			system.cur = newProgress;
		}
	}


	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}
}
