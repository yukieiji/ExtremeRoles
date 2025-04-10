using AmongUs.GameOptions;
using UnityEngine;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System;
using System.Collections.Generic;

using ExtremeRoles.Module;

using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;

namespace ExtremeRoles.Roles.Combination.DetectiveOffice;

#nullable enable

public sealed class Crime(byte killer, Vector2 pos)
{
	public float SearchTimer { get; private set; }

	public byte KillerId { get; } = killer;
	public Vector2 Pos { get; } = pos;

	public Detective.SearchCond Cond { get; set; }

	private float reportTimer;

	private CrimerInfo? Info
	{
		get
		{
			if ((
					!info.HasValue &&
					ExtremeRolesPlugin.ShipState.DeadPlayerInfo.TryGetValue(this.KillerId, out var state) &&
					state is not null &&
					state.Killer != null &&
					ExtremeRoleManager.TryGetRole(state.Killer.PlayerId, out var role)
				))
			{
				info = new CrimerInfo(
					state.DeadTime,
					this.reportTimer,
					state.Reason,
					role.Team, role.Id,
					role.Id == ExtremeRoleId.VanillaRole ?
						((Solo.VanillaRoleWrapper)role).VanilaRoleId : RoleTypes.Crewmate);
			}
			return info;
		}
	}
	private CrimerInfo? info;
	private Arrow? arrow;

	public void Clear()
	{
		this.arrow?.Clear();
		this.arrow = null;
	}

	public void ArrowSetActive(bool active)
	{
		if (this.arrow == null)
		{
			this.arrow = new Arrow(ColorPalette.DetectiveApprenticeKonai);
			this.arrow.UpdateTarget(this.Pos);
		}
		this.arrow.SetActive(active);
	}

	public void UpdateSearchTimer()
	{
		this.SearchTimer += Time.fixedDeltaTime;
	}

	public void Update()
	{
		this.reportTimer += Time.fixedDeltaTime;
	}
}

public readonly record struct CrimerInfo(
	DateTime KilledTime,
	float ReportTime,
	PlayerStatus Reason,
	ExtremeRoleType KillerTeam,
	ExtremeRoleId KillerRole,
	RoleTypes KillerVanillaRole);

public readonly record struct CrimeInfoOld(
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

	public CrimeInfoOld? GetCrimeInfo(byte playerId)
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

		return new CrimeInfoOld(
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

public sealed class DetectiveStatus() : IStatusModel
{
	public int CrimeSize => this.allCrime.Count;

	private readonly Dictionary<byte, Crime> allCrime = new Dictionary<byte, Crime>();

	public void Clear()
		=> this.allCrime.Clear();

	public bool TryGetCrime(byte playerId, out Crime? crime)
		=> this.allCrime.TryGetValue(playerId, out crime);

	public void ArrowSetActive(bool isActive)
	{
		foreach (var crime in this.allCrime.Values)
		{
			crime.ArrowSetActive(isActive);
		}
	}

	public void Upate()
	{
		foreach (var crime in this.allCrime.Values)
		{
			crime.Update();
		}
	}
}
