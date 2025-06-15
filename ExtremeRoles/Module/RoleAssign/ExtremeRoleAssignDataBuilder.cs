
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

public class ExtremeRoleAssignDataBuilder : IRoleAssignDataBuilder
{
	public enum Priority
	{
		Combination,
		Single,
		Not = 100,
	}

	private readonly IRoleAssignDataPreparer preparer;
	private readonly IRoleAssignDataBuildBehaviour[] behaviour;
	private readonly IAssignFilterInitializer assignFilterInitializer; // 追加
	private readonly IRoleAssignValidator validator; // 追加

	public ExtremeRoleAssignDataBuilder(
		IServiceProvider provider,
		IRoleAssignDataPreparer preparer,
		IAssignFilterInitializer assignFilterInitializer, // 追加
		IRoleAssignValidator validator // 追加
	)
	{
		this.preparer = preparer;
		this.behaviour = provider.GetServices<IRoleAssignDataBuildBehaviour>()
			.OrderByDescending(x => x.Priority)
			.ToArray();
		this.assignFilterInitializer = assignFilterInitializer; // 追加
		this.validator = validator; // 追加
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

		// RoleAssignFilter.Instance.Initialize(); // 削除

		do
		{
			this.assignFilterInitializer.Initialize(RoleAssignFilter.Instance, prepareData);

			foreach (var beha in this.behaviour)
			{
				beha.Build(in prepareData);
			}

		} while (this.validator.IsReBuild(prepareData));

		return prepareData.Assign.Data;
	}
}
