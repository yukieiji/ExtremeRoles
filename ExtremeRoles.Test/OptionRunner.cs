using System;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Test;

internal sealed class OptionRunner
	: TestRunnerBase
{
	private const int iteration = 100000;


	public override void Run()
	{
		this.Log.LogInfo($"----- Start: Options Test -----");
		for (int i = 0; i < iteration; ++i)
		{
			this.Log.LogInfo($"Update.Option.Iteration.{i}");
			this.updateRandom();

			this.Log.LogInfo($"Load.ClassicGameModeShipGlobalOption.Iteration.{i}");
			var classic = this.loadClassicGameModeShipGlobalOption();

			this.Log.LogInfo($"Load.ClassicGameModeShipGlobalOptionChangeTask.Iteration.{i}");
			this.loadIShipGlobalOptionChangeTask(classic);

			this.Log.LogInfo($"Load.HideNSeekModeShipGlobalOption.Iteration.{i}");
			var hns = this.loadHideNSeekShipGlobalOption();

			this.Log.LogInfo($"Load.HideNSeekModeShipGlobalOptionChangeTask.Iteration.{i}");
			this.loadIShipGlobalOptionChangeTask(hns);

			this.Log.LogInfo($"Load.CombinationRole.Iteration.{i}");
			loadCombinationRole();

			this.Log.LogInfo($"Load.NormalRole.Iteration.{i}");
			loadNormalRole();

			this.Log.LogInfo($"Load.GhostRole.Iteration.{i}");
			loadGhostRole();
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

				foreach (var opt in cate.Options)
				{
					int newIndex = RandomGenerator.Instance.Next(0, opt.Range);
					try
					{
						mng.Update(cate, opt, newIndex);
					}
					catch (Exception ex)
					{
						this.Log.LogError($"{opt.Info.Name} : {newIndex}   {ex.Message}");
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
			this.Log.LogError(ex.Message);
		}
		return null;
	}

	private void loadIShipGlobalOptionChangeTask(IShipGlobalOption? opt)
	{
		if (opt == null)
		{
			this.Log.LogWarning("Skip Load.IShipGlobalOptionChangeTask");
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
			this.Log.LogError(ex.Message);
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
			this.Log.LogError(ex.Message);
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
				this.Log.LogError($"{role}   {ex.Message}");
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
				this.Log.LogError($"{role}   {ex.Message}");
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
				this.Log.LogError($"{role}   {ex.Message}");
			}
		}
	}
}
