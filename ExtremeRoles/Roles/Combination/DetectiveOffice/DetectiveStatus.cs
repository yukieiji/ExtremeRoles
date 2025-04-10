using AmongUs.GameOptions;
using UnityEngine;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System;
using System.Collections.Generic;

using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;

namespace ExtremeRoles.Roles.Combination.DetectiveOffice;

public readonly record struct CrimeInfo(
	Vector2 Pos,
	DateTime KilledTime,
	float ReportTime,
	PlayerStatus Reason,
	ExtremeRoleType KillerTeam,
	ExtremeRoleId KillerRole,
	RoleTypes KillerVanillaRole,
	byte Killer);


public class CrimeInfoContainer
{
	private readonly Dictionary<byte, Vector2> deadBodyPos = new Dictionary<byte, Vector2>();
	private readonly Dictionary<byte, float> timer = new Dictionary<byte, float>();

	public CrimeInfoContainer()
	{
		Clear();
	}

	public void Clear()
	{
		deadBodyPos.Clear();
		timer.Clear();
	}

	public void AddDeadBody(
		PlayerControl killerPlayer,
		PlayerControl deadPlayer)
	{
		deadBodyPos.Add(
			deadPlayer.PlayerId,
			deadPlayer.GetTruePosition());
		timer.Add(
			deadPlayer.PlayerId,
			0.0f);
	}

	public CrimeInfo? GetCrimeInfo(byte playerId)
	{
		if (!(
				deadBodyPos.TryGetValue(playerId, out var pos) &&
				ExtremeRolesPlugin.ShipState.DeadPlayerInfo.TryGetValue(playerId, out var state) &&
				state is not null &&
				state.Killer != null &&
				ExtremeRoleManager.TryGetRole(state.Killer.PlayerId, out var role)
			))
		{
			return null;
		}

		return new CrimeInfo(
			pos, state.DeadTime,
			timer[playerId],
			state.Reason,
			role.Team, role.Id,
			role.Id == ExtremeRoleId.VanillaRole ?
				((Solo.VanillaRoleWrapper)role).VanilaRoleId : RoleTypes.Crewmate,
			state.Killer.PlayerId);
	}

	public void Update()
	{

		if (timer.Count == 0) { return; }

		foreach (byte playerId in timer.Keys)
		{
			timer[playerId] = timer[playerId] += Time.deltaTime;
		}
	}
}

public sealed class DetectiveStatus : IStatusModel
{
}
