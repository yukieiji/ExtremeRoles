using System;
using System.Net;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.Update;


namespace ExtremeRoles.Module.ApiHandler;

#nullable enable

public enum PostExRAssignOps
{
	FilterNewAdd,
	FilterRoleAdd,
	FilterAssignNumIncrease,
	FilterAssignNumDecrease,
	FilterRoleDelete,
	FilterDelete,
}

public readonly record struct DeltRoleAssignFilter(PostExRAssignOps Op, Guid FilterId, int? MapRoleId);

public sealed class PostExRAssignFilter : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost ||
			LobbyBehaviour.Instance == null)
		{
			IRequestHandler.SetStatusNG(response);
			response.Close();
			return;
		}

		var delta = IRequestHandler.DeserializeJson<DeltRoleAssignFilter>(context.Request);
		var filter = RoleAssignFilter.Instance;

		filter.Model.Initialize();
		switch (delta.Op)
		{
			case PostExRAssignOps.FilterNewAdd:
				if (filter.Model.FilterSet.ContainsKey(delta.FilterId))
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				RoleAssignFilterModelUpdater.AddFilter(filter.Model, delta.FilterId);
				break;
			case PostExRAssignOps.FilterRoleAdd:
				if (!(filter.Model.FilterSet.ContainsKey(delta.FilterId) && delta.MapRoleId.HasValue))
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				addFilterSetRole(delta.FilterId, delta.MapRoleId.Value);
				break;
			case PostExRAssignOps.FilterAssignNumIncrease:
				if (!(filter.Model.FilterSet.ContainsKey(delta.FilterId) && delta.MapRoleId.HasValue))
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				RoleAssignFilterModelUpdater.IncreaseFilterAssignNum(filter.Model, delta.FilterId);
				break;
			case PostExRAssignOps.FilterAssignNumDecrease:
				if (!(filter.Model.FilterSet.ContainsKey(delta.FilterId) && delta.MapRoleId.HasValue))
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				RoleAssignFilterModelUpdater.DecreaseFilterAssignNum(filter.Model, delta.FilterId);
				break;
			case PostExRAssignOps.FilterRoleDelete:
				if (!(filter.Model.FilterSet.ContainsKey(delta.FilterId) && delta.MapRoleId.HasValue))
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				RoleAssignFilterModelUpdater.RemoveFilterRole(filter.Model, delta.FilterId, delta.MapRoleId.Value);
				break;
			case PostExRAssignOps.FilterDelete:
				if (!filter.Model.FilterSet.ContainsKey(delta.FilterId))
				{
					IRequestHandler.SetStatusNG(response);
					response.Close();
					return;
				}
				RoleAssignFilterModelUpdater.RemoveFilter(filter.Model, delta.FilterId);
				break;
			default:
				IRequestHandler.SetStatusNG(response);
				response.Close();
				return;
		}
		
		filter.UpdateUi();

		IRequestHandler.SetStatusOK(response);
		response.Close();
	}
	private static void addFilterSetRole(Guid filterId, int id)
	{
		var model = RoleAssignFilter.Instance.Model;

		if (model.NormalRole.TryGetValue(id, out var roleId))
		{
			RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, roleId);
		}
		else if (model.CombRole.TryGetValue(id, out var combId))
		{
			RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, combId);
		}
		else if (model.GhostRole.TryGetValue(id, out var ghostId))
		{
			RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, ghostId);
		}
	}
}
