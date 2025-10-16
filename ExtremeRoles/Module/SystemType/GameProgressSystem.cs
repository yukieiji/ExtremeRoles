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
		RoleSetUpReady,
		RoleSetUpEnd,
		IntroEnd,
		PreTask,
		Task,
		Meeting,
		Exiled,
	}

	public static Progress Current
	{
		set => set(value);
	}

	private Progress cur = Progress.None;
	private bool isSetUpEnd = false;

	public static bool IsRoleSetUpEnd
		=> !(Is(Progress.None) || Is(Progress.IntroStart) || Is(Progress.RoleSetUpStart) || Is(Progress.RoleSetUpReady));

	public static bool IsGameNow
		=> IsTaskPhase || Is(Progress.Meeting) || Is(Progress.Exiled) || Is(Progress.PreTask);

	public static bool IsTaskPhase
		=> Is(Progress.Task) && PlayerControl.LocalPlayer != null;

	public static bool Is(Progress check)
	{
		var cur = ExtremeSystemTypeManager.Instance.TryGet<GameProgressSystem>(ExtremeSystemType.GameProgress, out var system) ? system.cur : Progress.None;
		
		return check switch
		{ 
			Progress.None => cur is Progress.None || PlayerControl.LocalPlayer == null || ShipStatus.Instance == null || !ShipStatus.Instance.enabled,
			Progress.IntroStart => isIntroCheck(cur, Progress.IntroStart),
			Progress.RoleSetUpStart => isIntroCheck(cur, Progress.RoleSetUpStart),
			Progress.RoleSetUpReady => isIntroCheck(cur, Progress.RoleSetUpReady),
			Progress.RoleSetUpEnd => isIntroCheck(cur, Progress.RoleSetUpEnd),
			Progress.IntroEnd => isIntroCheck(cur, Progress.IntroEnd),
			Progress.PreTask => (cur is Progress.PreTask || ExileController.Instance != null) && system.isSetUpEnd,
			Progress.Task => cur is Progress.Task && system.isSetUpEnd,
			Progress.Meeting => (cur is Progress.Meeting || MeetingHud.Instance != null || ExileController.Instance != null) && system.isSetUpEnd,
			Progress.Exiled => (cur is Progress.Exiled || ExileController.Instance != null) && system.isSetUpEnd,
			_ => cur == check,
		};
	}
	private static bool isIntroCheck(Progress cur, Progress target)
		=> cur == target || IntroCutscene.Instance != null;

	private static void set(Progress newProgress)
	{
		if (ExtremeSystemTypeManager.Instance.TryGet<GameProgressSystem>(ExtremeSystemType.GameProgress, out var system))
		{
			system.cur = newProgress;
			if (newProgress is Progress.RoleSetUpEnd)
			{
				system.isSetUpEnd = true;
			}
		}
	}


	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}
}
