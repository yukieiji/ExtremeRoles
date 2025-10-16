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
		Exiled,
	}

	public static Progress Current
	{
		private get => get();
		set => set(value);
	}

	private Progress cur = Progress.None;

	public static bool Is(Progress check)
	{
		var cur = get();
		return check switch
		{ 
			Progress.None => cur is Progress.None || PlayerControl.LocalPlayer == null || ShipStatus.Instance == null || !ShipStatus.Instance.enabled,
			Progress.IntroStart => isIntroCheck(cur, Progress.IntroStart),
			Progress.RoleSetUpStart => isIntroCheck(cur, Progress.RoleSetUpStart),
			Progress.RoleSetUpEnd => isIntroCheck(cur, Progress.RoleSetUpEnd),
			Progress.IntroEnd => isIntroCheck(cur, Progress.IntroEnd),
			Progress.Meeting => cur is Progress.Meeting || MeetingHud.Instance != null || ExileController.Instance != null,
			Progress.Exiled => cur is Progress.Exiled || ExileController.Instance != null,
			Progress.PreTask => cur is Progress.PreTask || ExileController.Instance != null,
			_ => cur == check,
		};
	}
	private static bool isIntroCheck(Progress cur, Progress target)
		=> cur == target || IntroCutscene.Instance != null;

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
