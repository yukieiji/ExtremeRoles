using ExtremeRoles.GhostRoles;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

using System;
using System.Collections.Generic;
using System.Net;

namespace ExtremeRoles.Module.ApiHandler;

public sealed class GetTranslationRole : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private void requestAction(HttpListenerContext context)
	{
		var response = context.Response;
		var results = new List<GetTranslationResponse>();

		foreach (var (id, role) in ExtremeRoleManager.NormalRole)
		{
			var roleId = (ExtremeRoleId)id;
			if (roleId is ExtremeRoleId.Null or ExtremeRoleId.VanillaRole)
			{
				continue;
			}

			string key = roleId.ToString();
			string translated = role.GetColoredRoleName(true);
			results.Add(new GetTranslationResponse(key, Array.Empty<object>(), translated));
		}

		foreach (var (id, role) in ExtremeGhostRoleManager.AllGhostRole)
		{
			if (id is ExtremeGhostRoleId.VanillaRole)
			{
				continue;
			}

			string key = id.ToString();
			string translated = role.GetColoredRoleName();
			results.Add(new GetTranslationResponse(key, Array.Empty<object>(), translated));
		}

		// リベラルだけ別管理のため
		addExtremeRoleIdRole(results, ExtremeRoleId.Leader);
		addExtremeRoleIdRole(results, ExtremeRoleId.Dove);
		addExtremeRoleIdRole(results, ExtremeRoleId.Militant);

		foreach (var (id, role) in ExtremeRoleManager.CombRole)
		{
			var roleId = (CombinationRoleType)id;
			string key = roleId.ToString();
			string translated = role.GetOptionName();
			// いにしえのコードの弊害
			results.Add(new GetTranslationResponse(key, Array.Empty<object>(), translated));
		}

		IRequestHandler.SetStatusOK(response);
		IRequestHandler.SetContentsType(response);
		IRequestHandler.Write(response, results.ToArray());
	}

	private static void addExtremeRoleIdRole(List<GetTranslationResponse> results, ExtremeRoleId roleId)
	{
		string key = roleId.ToString();
		string translated = Design.ColoredString(ColorPalette.LiberalColor, Tr.GetString(key));
		results.Add(new GetTranslationResponse(key, Array.Empty<object>(), translated));
	}
}
