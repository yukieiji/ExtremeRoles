using System;
using System.Collections.Generic;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using ExtremeRoles.Test.Helper;

namespace ExtremeRoles.Test.Lobby;

public class OptionRunner
	: LobbyTestRunnerBase
{
#if RELEASE
	private const int iteration = 3;
#endif
#if DEBUG
	private const int iteration = 10;
#endif

	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Start: Options Test -----");
		for (int i = 0; i < iteration; ++i)
		{
			Log.LogInfo($"Update.Option.Iteration.{i}");
			updateRandom();

			Log.LogInfo($"Load.ClassicGameModeShipGlobalOption.Iteration.{i}");
			var classic = loadClassicGameModeShipGlobalOption();

			Log.LogInfo($"Load.ClassicGameModeShipGlobalOptionChangeTask.Iteration.{i}");
			loadIShipGlobalOptionChangeTask(classic);

			Log.LogInfo($"Load.HideNSeekModeShipGlobalOption.Iteration.{i}");
			var hns = loadHideNSeekShipGlobalOption();

			Log.LogInfo($"Load.HideNSeekModeShipGlobalOptionChangeTask.Iteration.{i}");
			loadIShipGlobalOptionChangeTask(hns);

			Log.LogInfo($"Load.CombinationRole.Iteration.{i}");
			loadCombinationRole();

			Log.LogInfo($"Load.NormalRole.Iteration.{i}");
			loadNormalRole();

			Log.LogInfo($"Load.GhostRole.Iteration.{i}");
			loadGhostRole();

			yield return GameUtility.WaitForStabilize();
		}
	}

	private void updateRandom()
	{
		var mng = OptionManager.Instance;
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			if (!mng.TryGetTab(tab, out var tabObj))
			{
				continue;
			}

			foreach (var cate in tabObj.Category)
			{
				if (cate.Id == 0) { continue; }

				foreach (var opt in cate.Loader.Options)
				{
					int newIndex = RandomGenerator.Instance.Next(0, opt.Range);
					try
					{
						mng.UpdateToStep(cate, opt, newIndex);
					}
					catch (Exception ex)
					{
						Log.LogError($"{opt.Info.Name} : {newIndex}   {ex.Message}");
					}
				}
			}
		}
	}

	private IShipGlobalOption? loadClassicGameModeShipGlobalOption()
	{
		try
		{
			var opt = new ClassicGameModeShipGlobalOption();
			opt.Load();
			return opt;
		}
		catch (Exception ex)
		{
			Log.LogError(ex.Message);
		}
		return null;
	}

	private void loadIShipGlobalOptionChangeTask(IShipGlobalOption? opt)
	{
		if (opt == null)
		{
			Log.LogWarning("Skip Load.IShipGlobalOptionChangeTask");
			return;
		}
		try
		{
			var tasks = opt.ChangeTask;
			if (tasks is not IReadOnlySet<TaskTypes>)
			{
				throw new Exception("Task set ignore");
			}
		}
		catch (Exception ex)
		{
			Log.LogError(ex.Message);
		}
	}

	private IShipGlobalOption? loadHideNSeekShipGlobalOption()
	{
		try
		{
			var opt = new HideNSeekModeShipGlobalOption();
			opt.Load();
			return opt;
		}
		catch (Exception ex)
		{
			Log.LogError(ex.Message);
		}
		return null;
	}
	private void loadCombinationRole()
	{
		foreach (var role in ExtremeRoleManager.CombRole.Values)
		{
			try
			{
				role.Initialize();
			}
			catch (Exception ex)
			{
				Log.LogError($"{role}   {ex.Message}");
			}
		}
	}
	private void loadNormalRole()
	{
		foreach (var role in ExtremeRoleManager.NormalRole.Values)
		{
			try
			{
				role.Initialize();
			}
			catch (Exception ex)
			{
				Log.LogError($"{role}   {ex.Message}");
			}
		}
	}
	private void loadGhostRole()
	{
		foreach (var role in ExtremeGhostRoleManager.AllGhostRole.Values)
		{
			try
			{
				role.Initialize();
			}
			catch (Exception ex)
			{
				Log.LogError($"{role}   {ex.Message}");
			}
		}
	}
}
