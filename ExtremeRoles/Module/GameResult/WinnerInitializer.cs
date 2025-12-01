using System;
using System.Collections.Generic;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Module.GameResult;

public readonly record struct NeutralRoleInfo(NetworkedPlayerInfo Player, SingleRoleBase Role);
public readonly record struct WinModRoleInfo(NetworkedPlayerInfo Player, IRoleWinPlayerModifier Role);
public readonly record struct GhostRoleWinInfo(NetworkedPlayerInfo Player, IGhostRoleWinable Role);

public readonly record struct WinnerState(
	IReadOnlyList<NeutralRoleInfo> NeutralNoWinner,
	IReadOnlyList<WinModRoleInfo> ModRole,
	IReadOnlyList<GhostRoleWinInfo> GhostRoleWinCheck);

public readonly record struct InitializeResult(
	WinnerState Winner,
	IReadOnlyList<FinalSummary.PlayerSummary> Summary);

public sealed class WinnerInitializer(PlayerSummaryBuilder builder) : IDisposable
{
	private readonly PlayerSummaryBuilder builder = builder;

	public InitializeResult Initialize(WinnerContainer tempData)
	{
		int playerNum = GameData.Instance.AllPlayers.Count;

		var summaries = new List<FinalSummary.PlayerSummary>(playerNum);

		var neutralNoWinner = new List<NeutralRoleInfo>(playerNum);
	    var modRole = new List<WinModRoleInfo>(playerNum);
		var ghostWinCheckRole = new List<GhostRoleWinInfo>(playerNum);

		var logger = ExtremeRolesPlugin.Logger;

		foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			byte playerId = playerInfo.PlayerId;
			if (!ExtremeRoleManager.TryGetRole(playerId, out var role))
			{
				continue;
			}

			string playerName = playerInfo.PlayerName;
			if (role.IsNeutral())
			{
				if (ExtremeRoleManager.IsAliveWinNeutral(role, playerInfo))
				{
					logger.LogInfo($"AddPlusWinner(Reason:Alive Win) : {playerName}");
					tempData.AddPlusWinner(playerInfo);
				}
				else
				{
					neutralNoWinner.Add(new (playerInfo, role));
				}
				logger.LogInfo($"Remove Winner(Reason:Neutral) : {playerName}");
				tempData.Remove(playerInfo);
			}
			else if (role.IsLiberal())
			{
				logger.LogInfo($"Remove Winner(Reason:Liberal) : {playerName}");
				tempData.Remove(playerInfo);
			}
			else if (role.Core.Id is ExtremeRoleId.Xion)
			{
				logger.LogInfo($"Remove Winner(Reason:Xion Player) : {playerName}");
				tempData.Remove(playerInfo);
			}

			if (role is IRoleWinPlayerModifier winModRole)
			{
				modRole.Add(new(playerInfo, winModRole));
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is IRoleWinPlayerModifier multiWinModRole)
			{
				modRole.Add(new (playerInfo, multiWinModRole));
			}

			if (ExtremeGhostRoleManager.GameRole.TryGetValue(
					playerId, out GhostRoleBase? ghostRole) &&
				ghostRole is not null &&
				ghostRole.IsNeutral() &&
				ghostRole is IGhostRoleWinable winCheckGhostRole)
			{
				ghostWinCheckRole.Add(new (playerInfo, winCheckGhostRole));
			}

			var summary = this.builder.Create(
				playerInfo, role, ghostRole);
			if (summary.HasValue)
			{
				summaries.Add(summary.Value);
			}
		}

		return new InitializeResult(
			new WinnerState(neutralNoWinner, modRole, ghostWinCheckRole),
			summaries);
	}

	public void Dispose()
	{
		this.builder.Dispose();
	}
}
