
using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles; // ExtremeRoleIdのため (元からあったか確認)

using Microsoft.Extensions.DependencyInjection;

// IAssignFilterInitializer, IRoleAssignValidator, RoleAssignFilter, PreparationData が ExtremeRoles.Module.RoleAssign 名前空間にあると想定

namespace ExtremeRoles.Module.RoleAssign;

#nullable enable

public sealed class ExtremeRoleAssignDataBuilder : IRoleAssignDataBuilder
{
	public enum Priority
	{
		Combination,
		Single,
		Not = 100,
	}

	private readonly IRoleAssignDataPreparer preparer;
	private readonly IRoleAssignDataBuildBehaviour[] behaviour;
	private readonly IRoleAssignDataBuildBehaviour? vanillaFallBack;
	private readonly IAssignFilterInitializer assignFilterInitializer;
	private readonly IRoleAssignValidator validator;

	public ExtremeRoleAssignDataBuilder(
		IServiceProvider provider,
		IRoleAssignDataPreparer preparer,
		IAssignFilterInitializer assignFilterInitializer, // 追加
		IRoleAssignValidator validator // 追加
	)
	{
		this.preparer = preparer;

		var allBehave = provider.GetServices<IRoleAssignDataBuildBehaviour>();

		this.behaviour = allBehave
			.Where(x => x.Priority != (int)Priority.Not)
			.OrderBy(x => x.Priority)
			.ToArray();
		this.vanillaFallBack = allBehave.FirstOrDefault(x => x.Priority == (int)Priority.Not);

		this.assignFilterInitializer = assignFilterInitializer;
		this.validator = validator;
	}


	public IReadOnlyList<IPlayerToExRoleAssignData> Build()
	{
		var prepareData = this.preparer.Prepare();

		Logging.Debug(prepareData.RoleSpawn.ToString());
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

		GhostRoleSpawnDataManager.Instance.Create(prepareData.RoleSpawn.UseGhostCombRole);

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
