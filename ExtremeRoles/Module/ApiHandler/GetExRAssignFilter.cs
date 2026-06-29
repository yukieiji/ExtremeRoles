using System;
using System.Collections.Generic;
using System.Net;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public readonly record struct RoleAssignFilterDto(
	IReadOnlyDictionary<Guid, RoleAssignFilterSetDto> FilterSet,
	IReadOnlyList<int> FilterRoleId,
	IReadOnlyDictionary<int, ExtremeRoleId> NormalRoleId,
	IReadOnlyDictionary<int, CombinationRoleType> CombinationId,
	IReadOnlyDictionary<int, ExtremeGhostRoleId> GhostRoleId);
public readonly record struct RoleAssignFilterSetDto(
	int AssignNum,
	IReadOnlyDictionary<int, ExtremeRoleId> FilterNormalId,
	IReadOnlyDictionary<int, CombinationRoleType> FilterCombinationId,
	IReadOnlyDictionary<int, ExtremeGhostRoleId> FilterGhostRoleId);


public sealed class GetExRAssignFilter : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, getRoleAssignFilter());
	}

	private static RoleAssignFilterDto getRoleAssignFilter()
	{
		var model = RoleAssignFilter.Instance.Model;

		model.Initialize();

		return new RoleAssignFilterDto(
			createRoleAssignFilterSetDto(model.FilterSet),
			model.Id,
			model.NormalRole,
			model.CombRole,
			model.GhostRole);
	}

	private static IReadOnlyDictionary<Guid, RoleAssignFilterSetDto> createRoleAssignFilterSetDto(IReadOnlyDictionary<Guid, RoleFilterData> filterSet)
	{
		var result = new Dictionary<Guid, RoleAssignFilterSetDto>(filterSet.Count);

		foreach (var (key, item) in filterSet)
		{
			result[key] = new RoleAssignFilterSetDto(item.AssignNum, item.FilterNormalId, item.FilterCombinationId, item.FilterGhostRole);
		}

		return result;
	}
}
