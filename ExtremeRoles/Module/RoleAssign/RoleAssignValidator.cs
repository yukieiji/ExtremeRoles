using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Helper;

using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;


public class RoleAssignValidator(IServiceProvider provider) : IRoleAssignValidator
{
    private readonly IEnumerable<IRoleAssignDataChecker> checkers = provider.GetServices<IRoleAssignDataChecker>();

	public bool IsReBuild(in PreparationData prepareData)
	{
		Logging.Debug("------ RoleAssignValidator.IsReBuild START ------");

		if (!this.checkers.Any())
		{
			Logging.Debug("No checker defined. Skipping validation.");
			Logging.Debug("--- RoleAssignValidator.IsReBuild END (No checker) ---");
			return false;
		}

		bool isUpdate = false;
		foreach (var checker in this.checkers)
		{
			foreach (var ng in checker.GetNgData(prepareData))
			{
				// NGになったデータを元にprepareのデータを更新
				isUpdate = true;
			}
		}

		return isUpdate;
	}
}
