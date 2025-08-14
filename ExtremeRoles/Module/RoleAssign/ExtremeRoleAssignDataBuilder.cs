using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.RoleAssign;

#nullable enable

public sealed class ExtremeRoleAssignDataBuilder : IRoleAssignDataBuilder
{
	public enum Priority
	{
		Combination, // Fast
		Single,
		Not = 100, // Last
	}
	private readonly IRoleAssignDataBuildBehaviour[] behaviour;
	private readonly IRoleAssignDataBuildBehaviour? vanillaFallBack;
	private readonly IAssignFilterInitializer assignFilterInitializer;
	private readonly IRoleAssignValidator validator;

	public ExtremeRoleAssignDataBuilder(
		IEnumerable<IRoleAssignDataBuildBehaviour> allBehave,
		IAssignFilterInitializer assignFilterInitializer,
		IRoleAssignValidator validator
	)
	{
		this.behaviour = allBehave
			.Where(x => x.Priority != (int)Priority.Not)
			.OrderBy(x => x.Priority) // Enum宣言したの順番になるようにするのでOrderBy
			.ToArray();
		this.vanillaFallBack = allBehave.FirstOrDefault(x => x.Priority == (int)Priority.Not);

		this.assignFilterInitializer = assignFilterInitializer;
		this.validator = validator;
	}


	public IReadOnlyList<IPlayerToExRoleAssignData> Build(in PreparationData prepareData)
	{
		var spawnDataManager = prepareData.RoleSpawn;

		Logging.Debug(spawnDataManager.ToString());
		Logging.Debug(prepareData.Limit.ToString());

		if (ExtremeGameModeManager.Instance.EnableXion)
		{
			PlayerControl loaclPlayer = PlayerControl.LocalPlayer;
			var assignData = prepareData.Assign;
			assignData.AddAssignData(
				new PlayerToSingleRoleAssignData(
					loaclPlayer.PlayerId,
					(int)ExtremeRoleId.Xion,
					assignData.ControlId));
			assignData.RemveFromPlayerControl(loaclPlayer);
		}

		GhostRoleSpawnDataManager.Instance.Create(spawnDataManager.UseGhostCombRole);

		do
		{
			this.assignFilterInitializer.Initialize(RoleAssignFilter.Instance, prepareData);

			foreach (var beha in this.behaviour)
			{
				beha.Build(in prepareData);
			}

		} while (this.validator.IsReBuild(prepareData));

		this.vanillaFallBack?.Build(in prepareData);

		return prepareData.Assign.Data;
	}
}
