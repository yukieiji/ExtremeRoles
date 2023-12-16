using System;
using System.Collections.Generic;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test;

internal class Options : TestRunnerBase
{
	private const int iteration = 100000;


	public override void Run()
	{
		this.Logger.LogInfo($"----- Start: Options Test -----");
		for (int i = 0; i < iteration; ++i)
		{
			this.Logger.LogInfo($"Update.Option.Iteration.{i}");
			this.updateRandom();

			this.Logger.LogInfo($"Load.ClassicGameModeShipGlobalOption.Iteration.{i}");
			var classic = this.loadClassicGameModeShipGlobalOption();

			this.Logger.LogInfo($"Load.ClassicGameModeShipGlobalOptionChangeTask.Iteration.{i}");
			this.loadIShipGlobalOptionChangeTask(classic);

			this.Logger.LogInfo($"Load.HideNSeekModeShipGlobalOption.Iteration.{i}");
			var hns = this.loadHideNSeekShipGlobalOption();

			this.Logger.LogInfo($"Load.HideNSeekModeShipGlobalOptionChangeTask.Iteration.{i}");
			this.loadIShipGlobalOptionChangeTask(hns);

			this.Logger.LogInfo($"Load.CombinationRole.Iteration.{i}");
			loadCombinationRole();

			this.Logger.LogInfo($"Load.NormalRole.Iteration.{i}");
			loadNormalRole();

			this.Logger.LogInfo($"Load.GhostRole.Iteration.{i}");
			loadGhostRole();
		}
	}

	public override void Export()
	{

	}

	private void updateRandom()
	{
		foreach (var opt in OptionManager.Instance.GetAllIOption())
		{
			int newIndex = RandomGenerator.Instance.Next(0, opt.ValueCount);
			try
			{
				opt.UpdateSelection(newIndex);
			}
			catch (Exception ex)
			{
				this.Logger.LogError($"{opt.Name} : {newIndex}   {ex.Message}");
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
			this.Logger.LogError(ex.Message);
		}
		return null;
	}

	private void loadIShipGlobalOptionChangeTask(IShipGlobalOption? opt)
	{
		if (opt == null)
		{
			this.Logger.LogWarning("Skip Load.IShipGlobalOptionChangeTask");
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
			this.Logger.LogError(ex.Message);
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
			this.Logger.LogError(ex.Message);
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
				this.Logger.LogError($"{role}   {ex.Message}");
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
				this.Logger.LogError($"{role}   {ex.Message}");
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
				this.Logger.LogError($"{role}   {ex.Message}");
			}
		}
	}
}
