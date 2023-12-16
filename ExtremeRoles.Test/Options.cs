using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Test;

internal class Options : TestRunnerBase
{
	private const int iteration = 10000;


	public override void Run()
	{
		this.Logger.LogInfo($"----- Start: Options Test -----");
		for (int i = 0; i < iteration; ++i)
		{
			this.Logger.LogInfo($"Update.Option.Iteration.{i}");
			this.updateRandom();

			this.Logger.LogInfo($"Load.ClassicGameModeShipGlobalOption.Iteration.{i}");
			this.loadClassicGameModeShipGlobalOption();

			this.Logger.LogInfo($"Load.HideNSeekModeShipGlobalOption.Iteration.{i}");
			this.loadHideNSeekShipGlobalOption();

			this.Logger.LogInfo($"Load.CombinationRole.Iteration.{i}");
			loadCombinationRole();

			this.Logger.LogInfo($"Load.CombinationRole.Iteration.{i}");
			loadNormalRole();

			this.Logger.LogInfo($"Load.CombinationRole.Iteration.{i}");
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

	private void loadClassicGameModeShipGlobalOption()
	{
		try
		{
			var opt = new ClassicGameModeShipGlobalOption();
			opt.Load();
		}
		catch (Exception ex)
		{
			this.Logger.LogError(ex.Message);
		}
	}
	private void loadHideNSeekShipGlobalOption()
	{
		try
		{
			var opt = new HideNSeekModeShipGlobalOption();
			opt.Load();
		}
		catch (Exception ex)
		{
			this.Logger.LogError(ex.Message);
		}
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
