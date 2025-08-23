using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Combination.DetectiveOffice;

#nullable enable

public sealed record Crime(
	byte Target,
	byte Killer,
	Vector2 Pos,
	ExtremeRoleType KillerTeam,
	ExtremeRoleId KillerRole,
	RoleTypes KillerVanillaRole)
{
	public CrimeInfo Info
	{
		get
		{
			if (info.HasValue)
			{
				return info.Value;
			}

			if ((					
					ExtremeRolesPlugin.ShipState.DeadPlayerInfo.TryGetValue(this.Target, out var state) &&
					state is not null &&
					state.Killer != null &&
					ExtremeRoleManager.TryGetRole(state.Killer.PlayerId, out var role)
				))
			{
				info = new CrimeInfo(
					this.Target,
					this.Killer,
					this.Pos,
					state.DeadTime,
					state.Reason,
					this.reportTime,
					role.Core.Team,
					role.Core.Id,
					role.Core.Id == ExtremeRoleId.VanillaRole ?
						((Solo.VanillaRoleWrapper)role).VanilaRoleId : RoleTypes.Crewmate);
				return info.Value;
			}
			throw new ArgumentException();
		}
	}
	private CrimeInfo? info;

	private float reportTime = 0f;

	public void Update(float deltaTime)
	{
		this.reportTime += deltaTime;
	}
}

public sealed class CrimeContainer()
{
	private readonly Dictionary<byte, Crime> crimerInfo = [];

	public void Clear()
	{
		this.crimerInfo.Clear();
	}

	public bool TryGet(byte target, [NotNullWhen(true)] out Crime? info)
		=> this.crimerInfo.TryGetValue(target, out info);

	public void Update(float deltaTime)
	{
		foreach (var info in this.crimerInfo.Values)
		{
			info.Update(deltaTime);
		}
	}

	public void Add(Crime info)
	{
		this.crimerInfo.Add(info.Target, info);
	}
}


public sealed class DetectiveStatus() : IStatusModel
{
	private readonly CrimeContainer container = new CrimeContainer();

	
	public void Clear()
		=> this.container.Clear();

	public bool TryGetCrime(byte playerId, [NotNullWhen(true)] out CrimeInfo crimeInfo)
	{
		if (!this.container.TryGet(playerId, out var info))
		{
			crimeInfo = default;
			return false;
		}
		crimeInfo = info.Info;
		return true;
	}

	public void AddCrime(PlayerControl killer, PlayerControl target)
	{
		if (!ExtremeRoleManager.TryGetRole(killer.PlayerId, out var role))
		{
			return;
		}

		this.container.Add(
			new Crime(
				target.PlayerId,
				killer.PlayerId,
				target.GetTruePosition(),
				role.Core.Team,
				role.Core.Id,
				killer.Data.Role.Role));
	}

	public void Upate(float deltaTime)
	{
		this.container.Update(deltaTime);
	}
}
